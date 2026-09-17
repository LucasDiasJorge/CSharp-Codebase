using System.Collections.Concurrent;
using DistributedCacheInvalidationDemo.Data;
using StackExchange.Redis;

namespace DistributedCacheInvalidationDemo.Cache;

/// <summary>
/// Um nó da aplicação: cache local próprio (L1) sobre um Redis compartilhado (L2).
/// Em produção cada nó seria um processo separado; aqui vários convivem no mesmo
/// processo para o exemplo caber em um comando. O que importa é que **cada nó tem o seu
/// L1**, e é justamente essa cópia local que fica velha quando outro nó escreve.
/// </summary>
public sealed class CacheNode : IAsyncDisposable
{
    public const string InvalidationChannel = "cache:invalidation";

    private readonly ConcurrentDictionary<string, LocalEntry> _localCache = new ConcurrentDictionary<string, LocalEntry>();
    private readonly IConnectionMultiplexer _redis;
    private readonly ProductRepository _repository;
    private readonly ILogger _logger;

    private int _localHits;
    private int _redisHits;
    private int _originReads;
    private int _invalidationsReceived;
    private int _invalidationsIgnored;

    public CacheNode(string nodeId, IConnectionMultiplexer redis, ProductRepository repository, ILogger logger)
    {
        NodeId = nodeId;
        _redis = redis;
        _repository = repository;
        _logger = logger;
    }

    public string NodeId { get; }

    public int LocalHits => Volatile.Read(ref _localHits);

    public int RedisHits => Volatile.Read(ref _redisHits);

    public int OriginReads => Volatile.Read(ref _originReads);

    public int InvalidationsReceived => Volatile.Read(ref _invalidationsReceived);

    /// <summary>Mensagens da própria origem, descartadas por já terem sido aplicadas.</summary>
    public int InvalidationsIgnored => Volatile.Read(ref _invalidationsIgnored);

    /// <summary>
    /// Assina o canal de invalidação. Sem esta assinatura, o L1 deste nó só se corrige
    /// quando a entrada expira por TTL — e até lá ele serve dado errado.
    /// </summary>
    public async Task SubscribeAsync()
    {
        ISubscriber subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Literal(InvalidationChannel),
            (_, message) => HandleInvalidation(message!)).ConfigureAwait(false);

        _logger.LogInformation("No {No} assinou {Canal}.", NodeId, InvalidationChannel);
    }

    public IReadOnlyDictionary<string, string> Snapshot()
    {
        Dictionary<string, string> snapshot = new Dictionary<string, string>();
        foreach (KeyValuePair<string, LocalEntry> pair in _localCache)
        {
            snapshot[pair.Key] = pair.Value.Value;
        }

        return snapshot;
    }

    /// <summary>Leitura em dois níveis: L1 local, depois Redis, depois a origem.</summary>
    public async Task<ReadResult> ReadAsync(string productId)
    {
        if (_localCache.TryGetValue(productId, out LocalEntry? local))
        {
            Interlocked.Increment(ref _localHits);

            return new ReadResult(local.Value, "L1 (memoria do no)");
        }

        IDatabase database = _redis.GetDatabase();
        RedisValue shared = await database.StringGetAsync(RedisKey(productId)).ConfigureAwait(false);

        if (shared.HasValue)
        {
            Interlocked.Increment(ref _redisHits);
            _localCache[productId] = new LocalEntry(shared!);

            return new ReadResult(shared!, "L2 (Redis)");
        }

        string? fromOrigin = _repository.Read(productId);
        if (fromOrigin is null)
        {
            return new ReadResult(null, "nao encontrado");
        }

        Interlocked.Increment(ref _originReads);

        await database.StringSetAsync(RedisKey(productId), fromOrigin, TimeSpan.FromMinutes(5)).ConfigureAwait(false);
        _localCache[productId] = new LocalEntry(fromOrigin);

        return new ReadResult(fromOrigin, "origem");
    }

    /// <summary>
    /// Escrita correta: grava na origem, invalida o L2 e **avisa todos os nós**. A ordem
    /// importa — publicar antes de gravar abriria uma janela em que outro nó recarrega o
    /// valor antigo e o coloca de volta no cache.
    /// </summary>
    public async Task WriteAsync(string productId, string value)
    {
        _repository.Write(productId, value);

        IDatabase database = _redis.GetDatabase();
        await database.KeyDeleteAsync(RedisKey(productId)).ConfigureAwait(false);

        // Invalidar em vez de reescrever: dois nós escrevendo em sequência poderiam
        // gravar valores em ordem trocada. Apagar faz o proximo leitor buscar na origem.
        _localCache.TryRemove(productId, out _);

        ISubscriber subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(
            RedisChannel.Literal(InvalidationChannel),
            $"{NodeId}|{productId}").ConfigureAwait(false);

        _logger.LogInformation("No {No} escreveu {ProdutoId} e publicou a invalidacao.", NodeId, productId);
    }

    /// <summary>
    /// A versão quebrada, para comparação: escreve sem avisar ninguém. O L1 dos outros
    /// nós continua servindo o valor antigo até expirar.
    /// </summary>
    public async Task WriteWithoutInvalidationAsync(string productId, string value)
    {
        _repository.Write(productId, value);

        await _redis.GetDatabase().KeyDeleteAsync(RedisKey(productId)).ConfigureAwait(false);
        _localCache.TryRemove(productId, out _);

        _logger.LogWarning("No {No} escreveu {ProdutoId} SEM publicar invalidacao.", NodeId, productId);
    }

    private void HandleInvalidation(string message)
    {
        string[] parts = message.Split('|');
        if (parts.Length != 2)
        {
            return;
        }

        string publisher = parts[0];
        string productId = parts[1];

        if (publisher == NodeId)
        {
            // Mensagem propria: este no ja removeu a entrada antes de publicar. Ignorar
            // e so higiene de contagem — reaplicar seria inofensivo.
            Interlocked.Increment(ref _invalidationsIgnored);

            return;
        }

        Interlocked.Increment(ref _invalidationsReceived);
        _localCache.TryRemove(productId, out _);

        _logger.LogInformation("No {No} invalidou {ProdutoId} por aviso de {Origem}.", NodeId, productId, publisher);
    }

    public void ClearLocal() => _localCache.Clear();

    private static string RedisKey(string productId) => $"produto:{productId}";

    public async ValueTask DisposeAsync()
    {
        await _redis.GetSubscriber().UnsubscribeAsync(RedisChannel.Literal(InvalidationChannel)).ConfigureAwait(false);
    }
}

public sealed record LocalEntry(string Value);

public sealed record ReadResult(string? Value, string Source);
