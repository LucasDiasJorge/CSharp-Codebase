using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqRequestReplyDemo.Client;

namespace RabbitMqRequestReplyDemo.Server;

/// <summary>
/// Lado do servidor. Consome da fila de requisições e responde para a fila indicada em
/// <c>ReplyTo</c>, repetindo o <c>CorrelationId</c> recebido — sem isso, o cliente não
/// consegue atribuir a resposta a nenhuma requisição.
/// </summary>
public sealed class RpcServer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private readonly string _hostName;
    private readonly ILogger<RpcServer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public RpcServer(IConfiguration configuration, ILogger<RpcServer> logger)
    {
        _hostName = configuration["RabbitMq:Host"] ?? "localhost";
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConnectionFactory factory = new ConnectionFactory { HostName = _hostName };
        _connection = await factory.CreateConnectionAsync(stoppingToken).ConfigureAwait(false);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken).ConfigureAwait(false);

        await _channel.QueueDeclareAsync(
            queue: RpcClient.RequestQueue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken).ConfigureAwait(false);

        await _channel.BasicQosAsync(0, prefetchCount: 4, global: false, cancellationToken: stoppingToken).ConfigureAwait(false);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleRequestAsync;

        await _channel.BasicConsumeAsync(
            queue: RpcClient.RequestQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("Servidor RPC ouvindo {Fila}.", RpcClient.RequestQueue);

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default).ConfigureAwait(false);
    }

    private async Task HandleRequestAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null)
        {
            return;
        }

        string body = Encoding.UTF8.GetString(args.Body.ToArray());
        string? correlationId = args.BasicProperties.CorrelationId;
        string? replyTo = args.BasicProperties.ReplyTo;

        RpcRequest? request = null;

        try
        {
            request = JsonSerializer.Deserialize<RpcRequest>(body, JsonOptions);
        }
        catch (JsonException)
        {
            // Segue adiante e responde com erro: deixar a requisicao sem resposta faria o
            // cliente esperar ate o timeout por algo que ja se sabe que nao vira.
        }

        string response;

        if (request is null)
        {
            response = JsonSerializer.Serialize(new { error = "payload invalido" }, JsonOptions);
        }
        else
        {
            // Atraso controlado pelo proprio pedido: e assim que o cenario de timeout
            // pode ser disparado por uma requisicao HTTP.
            if (request.DelayMs > 0)
            {
                await Task.Delay(request.DelayMs).ConfigureAwait(false);
            }

            response = JsonSerializer.Serialize(new
            {
                result = request.A + request.B,
                processedAt = DateTimeOffset.UtcNow,
                delayedMs = request.DelayMs
            }, JsonOptions);
        }

        if (replyTo is not null)
        {
            BasicProperties properties = new BasicProperties
            {
                // Devolver o MESMO CorrelationId e a unica coisa que liga esta resposta a
                // requisicao original. Trocar ou omitir faz a resposta virar orfa.
                CorrelationId = correlationId
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: replyTo,
                mandatory: false,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(response)).ConfigureAwait(false);
        }
        else
        {
            _logger.LogWarning("Requisicao {Correlacao} sem ReplyTo: nao ha para onde responder.", correlationId);
        }

        await _channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync().ConfigureAwait(false);
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}

public sealed class RpcRequest
{
    public int A { get; set; }

    public int B { get; set; }

    /// <summary>Atraso artificial, para exercitar o timeout do cliente.</summary>
    public int DelayMs { get; set; }
}
