using System.Text;
using RabbitMQ.Client;

namespace TransactionalOutboxDemo.Messaging;

/// <summary>
/// Publicador real para o RabbitMQ, com um interruptor de falha. O interruptor existe
/// porque o padrão só faz sentido quando o broker está fora do ar — é o cenário que
/// separa o outbox do dual write.
/// </summary>
public sealed class RabbitMqPublisher : IAsyncDisposable
{
    public const string ExchangeName = "outbox-demo";

    private readonly SemaphoreSlim _connectionGate = new SemaphoreSlim(1, 1);
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly string _hostName;

    private IConnection? _connection;
    private IChannel? _channel;
    private bool _simulatedOutage;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _hostName = configuration["RabbitMq:Host"] ?? "localhost";
    }

    /// <summary>Broker "fora do ar" do ponto de vista da aplicação.</summary>
    public bool SimulatedOutage
    {
        get => _simulatedOutage;
        set
        {
            _simulatedOutage = value;
            _logger.LogWarning("Broker simulado agora esta {Estado}.", value ? "FORA DO AR" : "disponivel");
        }
    }

    public IReadOnlyList<PublishedMessage> Published => _published;

    private readonly List<PublishedMessage> _published = new List<PublishedMessage>();

    public async Task PublishAsync(Guid messageId, string type, string payload, CancellationToken cancellationToken)
    {
        if (_simulatedOutage)
        {
            throw new InvalidOperationException("Broker indisponivel (falha simulada).");
        }

        IChannel channel = await GetChannelAsync(cancellationToken).ConfigureAwait(false);

        BasicProperties properties = new BasicProperties
        {
            // MessageId viaja junto: e com ele que o consumidor descarta duplicatas.
            MessageId = messageId.ToString(),
            Type = type,
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: type,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(payload),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        lock (_published)
        {
            _published.Add(new PublishedMessage(messageId, type, DateTimeOffset.UtcNow));
        }

        _logger.LogInformation("Mensagem {MensagemId} do tipo {Tipo} publicada.", messageId, type);
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _connectionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            ConnectionFactory factory = new ConnectionFactory { HostName = _hostName };
            _connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            await _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return _channel;
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync().ConfigureAwait(false);
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        _connectionGate.Dispose();
    }
}

public sealed record PublishedMessage(Guid MessageId, string Type, DateTimeOffset PublishedAt);
