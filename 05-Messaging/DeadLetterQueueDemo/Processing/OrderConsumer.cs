using System.Text;
using DeadLetterQueueDemo.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DeadLetterQueueDemo.Processing;

/// <summary>
/// Consumidor da fila principal. Toda a política de retentativa vive aqui: quantas
/// vezes, com que atraso e quando desistir.
/// </summary>
public sealed class OrderConsumer : BackgroundService
{
    private readonly RabbitMqConnection _connection;
    private readonly OrderProcessor _processor;
    private readonly ProcessingLog _log;
    private readonly ILogger<OrderConsumer> _logger;

    private IChannel? _channel;

    public OrderConsumer(RabbitMqConnection connection, OrderProcessor processor, ProcessingLog log, ILogger<OrderConsumer> logger)
    {
        _connection = connection;
        _processor = processor;
        _log = log;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(stoppingToken).ConfigureAwait(false);
        await _connection.DeclareTopologyAsync(_channel, stoppingToken).ConfigureAwait(false);

        // Um de cada vez: sem isso o broker despeja a fila inteira no consumidor e o
        // controle de retentativa fica ilegivel no exemplo.
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken).ConfigureAwait(false);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: QueueTopology.MainQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("Consumidor ouvindo {Fila}.", QueueTopology.MainQueue);

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default).ConfigureAwait(false);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null)
        {
            return;
        }

        string payload = Encoding.UTF8.GetString(args.Body.ToArray());
        int attempt = ReadAttempt(args.BasicProperties) + 1;

        ProcessingResult result = _processor.Process(payload, attempt);
        _log.Record(payload, attempt, result);

        switch (result.Outcome)
        {
            case ProcessingOutcome.Success:
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);
                break;

            case ProcessingOutcome.PermanentFailure:
                // Direto para a DLQ, sem gastar retentativa: o resultado nao vai mudar.
                _logger.LogWarning("Falha permanente na tentativa {Tentativa}: {Motivo}. Direto para a DLQ.", attempt, result.Detail);
                await SendToDeadLetterAsync(payload, attempt, result.Detail).ConfigureAwait(false);
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);
                break;

            case ProcessingOutcome.TransientFailure when attempt >= QueueTopology.MaxAttempts:
                // Esgotou o limite. Sem o limite, uma poison message ficaria em ciclo
                // para sempre, ocupando o consumidor e atrasando todo o resto.
                _logger.LogWarning(
                    "Tentativas esgotadas ({Tentativa}/{Maximo}): {Motivo}. Enviando para a DLQ.",
                    attempt,
                    QueueTopology.MaxAttempts,
                    result.Detail);

                await SendToDeadLetterAsync(payload, attempt, $"tentativas esgotadas: {result.Detail}").ConfigureAwait(false);
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);
                break;

            case ProcessingOutcome.TransientFailure:
                _logger.LogInformation(
                    "Falha transitoria na tentativa {Tentativa}/{Maximo}: {Motivo}. Reagendando em {Atraso}ms.",
                    attempt,
                    QueueTopology.MaxAttempts,
                    result.Detail,
                    QueueTopology.RetryDelayMs);

                await SendToRetryAsync(payload, attempt).ConfigureAwait(false);

                // Ack na principal: a mensagem ja foi copiada para a fila de espera.
                // BasicNack(requeue: true) aqui devolveria a mensagem imediatamente e
                // criaria o ciclo infinito que este projeto existe para evitar.
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);
                break;
        }
    }

    private async Task SendToRetryAsync(string payload, int attempt)
    {
        BasicProperties properties = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object?> { [QueueTopology.AttemptHeader] = attempt }
        };

        await _channel!.BasicPublishAsync(
            exchange: QueueTopology.RetryExchange,
            routingKey: QueueTopology.RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(payload)).ConfigureAwait(false);
    }

    private async Task SendToDeadLetterAsync(string payload, int attempt, string reason)
    {
        BasicProperties properties = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object?>
            {
                [QueueTopology.AttemptHeader] = attempt,

                // O motivo viaja junto. Uma DLQ sem o porquê de cada mensagem obriga a
                // reconstruir o incidente a partir do log, quando ele ainda existir.
                [QueueTopology.ReasonHeader] = reason
            }
        };

        await _channel!.BasicPublishAsync(
            exchange: QueueTopology.DeadLetterExchange,
            routingKey: QueueTopology.RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(payload)).ConfigureAwait(false);
    }

    private static int ReadAttempt(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is null || !properties.Headers.TryGetValue(QueueTopology.AttemptHeader, out object? value))
        {
            return 0;
        }

        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out int parsed) => parsed,
            _ => 0
        };
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync().ConfigureAwait(false);
        }

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
