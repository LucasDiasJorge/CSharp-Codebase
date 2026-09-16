using RabbitMQ.Client;

namespace DeadLetterQueueDemo.Messaging;

/// <summary>
/// Conexão e declaração da topologia. As três filas são criadas no start para que o
/// exemplo não dependa de configuração manual no broker.
/// </summary>
public sealed class RabbitMqConnection : IAsyncDisposable
{
    private readonly string _hostName;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

    private IConnection? _connection;

    public RabbitMqConnection(IConfiguration configuration, ILogger<RabbitMqConnection> logger)
    {
        _hostName = configuration["RabbitMq:Host"] ?? "localhost";
        _logger = logger;
    }

    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        IConnection connection = await GetConnectionAsync(cancellationToken).ConfigureAwait(false);

        return await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Declara exchanges e filas. A amarração é o coração do exemplo: a principal aponta
    /// para a DLX; a de espera aponta de volta para a principal.
    /// </summary>
    public async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(QueueTopology.MainExchange, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.ExchangeDeclareAsync(QueueTopology.RetryExchange, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.ExchangeDeclareAsync(QueueTopology.DeadLetterExchange, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken).ConfigureAwait(false);

        // Fila principal: o que for rejeitado sem requeue cai na DLX.
        await channel.QueueDeclareAsync(
            queue: QueueTopology.MainQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = QueueTopology.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = QueueTopology.RoutingKey
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        // Fila de espera: ninguem consome daqui. A mensagem expira pelo TTL e a propria
        // expiracao a devolve para a exchange principal. Atraso sem ocupar consumidor.
        await channel.QueueDeclareAsync(
            queue: QueueTopology.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-message-ttl"] = QueueTopology.RetryDelayMs,
                ["x-dead-letter-exchange"] = QueueTopology.MainExchange,
                ["x-dead-letter-routing-key"] = QueueTopology.RoutingKey
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        // DLQ: sem TTL e sem DLX. O que chega aqui fica ate alguem decidir o que fazer.
        await channel.QueueDeclareAsync(
            queue: QueueTopology.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await channel.QueueBindAsync(QueueTopology.MainQueue, QueueTopology.MainExchange, QueueTopology.RoutingKey, cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.QueueBindAsync(QueueTopology.RetryQueue, QueueTopology.RetryExchange, QueueTopology.RoutingKey, cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.QueueBindAsync(QueueTopology.DeadLetterQueue, QueueTopology.DeadLetterExchange, QueueTopology.RoutingKey, cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Topologia declarada: {Principal}, {Espera}, {Dlq}.", QueueTopology.MainQueue, QueueTopology.RetryQueue, QueueTopology.DeadLetterQueue);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            ConnectionFactory factory = new ConnectionFactory { HostName = _hostName };
            _connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);

            return _connection;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        _gate.Dispose();
    }
}
