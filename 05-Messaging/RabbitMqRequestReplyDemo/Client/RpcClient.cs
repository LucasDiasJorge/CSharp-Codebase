using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqRequestReplyDemo.Client;

/// <summary>
/// Lado do cliente do request/reply. Mensageria é assíncrona por natureza: a requisição
/// sai por uma fila e a resposta volta por outra, sem relação nenhuma entre as duas do
/// ponto de vista do broker. Quem reconstrói o par é o <c>CorrelationId</c>.
/// </summary>
public sealed class RpcClient : IAsyncDisposable
{
    public const string RequestQueue = "rpc.requests";

    /// <summary>
    /// Fila de resposta pseudo-exclusiva do próprio RabbitMQ. Usar <c>amq.rabbitmq.reply-to</c>
    /// evita declarar uma fila por cliente (ou, pior, por requisição): o broker cria um
    /// canal de resposta leve, sem custo de declaração nem de limpeza.
    /// </summary>
    public const string ReplyQueue = "amq.rabbitmq.reply-to";

    private readonly ConcurrentDictionary<string, PendingRequest> _pending = new ConcurrentDictionary<string, PendingRequest>();
    private readonly string _hostName;
    private readonly ILogger<RpcClient> _logger;

    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _initGate = new SemaphoreSlim(1, 1);

    private int _lateResponses;

    public RpcClient(IConfiguration configuration, ILogger<RpcClient> logger)
    {
        _hostName = configuration["RabbitMq:Host"] ?? "localhost";
        _logger = logger;
    }

    /// <summary>Requisições ainda aguardando resposta. Deveria voltar a zero sempre.</summary>
    public int PendingCount => _pending.Count;

    /// <summary>Respostas que chegaram depois do timeout e foram descartadas.</summary>
    public int LateResponses => Volatile.Read(ref _lateResponses);

    public async Task<RpcResult> CallAsync(string payload, TimeSpan timeout, CancellationToken cancellationToken)
    {
        IChannel channel = await EnsureChannelAsync(cancellationToken).ConfigureAwait(false);

        string correlationId = Guid.NewGuid().ToString("N");
        PendingRequest pending = new PendingRequest(correlationId);

        _pending[correlationId] = pending;

        BasicProperties properties = new BasicProperties
        {
            CorrelationId = correlationId,

            // Para onde o servidor deve responder. Sem ReplyTo, ele nao tem como saber.
            ReplyTo = ReplyQueue
        };

        DateTimeOffset sentAt = DateTimeOffset.UtcNow;

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: RequestQueue,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(payload),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        try
        {
            string response = await pending.Task.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);

            return RpcResult.FromResponse(response, DateTimeOffset.UtcNow - sentAt);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Requisicao {Correlacao} expirou apos {Timeout}ms.", correlationId, timeout.TotalMilliseconds);

            return RpcResult.FromTimeout(DateTimeOffset.UtcNow - sentAt);
        }
        finally
        {
            // Limpeza obrigatoria. Sem remover a entrada aqui, cada requisicao que expira
            // deixa um TaskCompletionSource preso no dicionario para sempre — vazamento
            // silencioso que so aparece como consumo de memoria crescente em producao.
            _pending.TryRemove(correlationId, out _);
        }
    }

    private async Task<IChannel> EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _initGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            ConnectionFactory factory = new ConnectionFactory { HostName = _hostName };
            _connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            await _channel.QueueDeclareAsync(
                queue: RequestQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += HandleReplyAsync;

            // autoAck obrigatorio na fila pseudo-exclusiva amq.rabbitmq.reply-to.
            await _channel.BasicConsumeAsync(
                queue: ReplyQueue,
                autoAck: true,
                consumer: consumer,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return _channel;
        }
        finally
        {
            _initGate.Release();
        }
    }

    private Task HandleReplyAsync(object sender, BasicDeliverEventArgs args)
    {
        string? correlationId = args.BasicProperties.CorrelationId;
        string body = Encoding.UTF8.GetString(args.Body.ToArray());

        if (correlationId is null || !_pending.TryRemove(correlationId, out PendingRequest? pending))
        {
            // Resposta orfa: chegou depois do timeout, ou de uma requisicao que ja foi
            // abandonada. Descartar em silencio e o comportamento correto — o que nao se
            // pode e deixar a excecao subir e derrubar o consumidor de respostas.
            Interlocked.Increment(ref _lateResponses);
            _logger.LogInformation("Resposta tardia para {Correlacao} descartada.", correlationId ?? "(sem correlationId)");

            return Task.CompletedTask;
        }

        pending.Complete(body);

        return Task.CompletedTask;
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

        _initGate.Dispose();
    }
}

/// <summary>Requisição aguardando resposta.</summary>
public sealed class PendingRequest
{
    private readonly TaskCompletionSource<string> _completion =
        new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

    public PendingRequest(string correlationId)
    {
        CorrelationId = correlationId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string CorrelationId { get; }

    public DateTimeOffset CreatedAt { get; }

    public Task<string> Task => _completion.Task;

    public void Complete(string response) => _completion.TrySetResult(response);
}

public sealed class RpcResult
{
    private RpcResult(bool answered, string? response, TimeSpan elapsed)
    {
        Answered = answered;
        Response = response;
        Elapsed = elapsed;
    }

    public bool Answered { get; }

    public string? Response { get; }

    public TimeSpan Elapsed { get; }

    public static RpcResult FromResponse(string response, TimeSpan elapsed) => new RpcResult(true, response, elapsed);

    public static RpcResult FromTimeout(TimeSpan elapsed) => new RpcResult(false, null, elapsed);
}
