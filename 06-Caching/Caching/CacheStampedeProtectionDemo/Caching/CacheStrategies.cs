using System.Collections.Concurrent;
using System.Text.Json;
using CacheStampedeProtectionDemo.Origin;
using StackExchange.Redis;

namespace CacheStampedeProtectionDemo.Caching;

/// <summary>Valor guardado no Redis, com os dois prazos do stale-while-revalidate.</summary>
public sealed class CachedEntry
{
    public string Value { get; set; } = string.Empty;

    /// <summary>Depois disto o valor é considerado velho, mas ainda servível.</summary>
    public DateTimeOffset SoftExpiresAt { get; set; }
}

/// <summary>
/// As três estratégias lado a lado. Todas usam o mesmo Redis e a mesma origem; o que
/// muda é o que acontece quando várias requisições erram o cache ao mesmo tempo.
/// </summary>
public sealed class CacheStrategies
{
    public static readonly TimeSpan BaseTtl = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan SoftTtl = TimeSpan.FromSeconds(3);

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// Um semáforo por chave. É o single-flight: quem chega primeiro vai à origem, os
    /// demais esperam e aproveitam o resultado. Note que isso coordena apenas ESTE
    /// processo — com várias instâncias, cada uma faz a sua chamada. Para coordenar
    /// entre instâncias seria preciso lock distribuído, assunto de RedisDistributedLockDemo.
    /// </summary>
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyGates = new ConcurrentDictionary<string, SemaphoreSlim>();

    /// <summary>Chaves com revalidação em andamento, para não disparar várias.</summary>
    private readonly ConcurrentDictionary<string, byte> _refreshing = new ConcurrentDictionary<string, byte>();

    private readonly IConnectionMultiplexer _redis;
    private readonly SlowDataSource _origin;
    private readonly ILogger<CacheStrategies> _logger;

    public CacheStrategies(IConnectionMultiplexer redis, SlowDataSource origin, ILogger<CacheStrategies> logger)
    {
        _redis = redis;
        _origin = origin;
        _logger = logger;
    }

    /// <summary>
    /// Cache-aside sem proteção. Cada requisição que erra o cache vai à origem por conta
    /// própria — é o stampede.
    /// </summary>
    public async Task<string> GetNaiveAsync(string key, CancellationToken cancellationToken)
    {
        IDatabase database = _redis.GetDatabase();
        RedisValue cached = await database.StringGetAsync(key).ConfigureAwait(false);

        if (cached.HasValue)
        {
            return Deserialize(cached!).Value;
        }

        string value = await _origin.LoadAsync(key, cancellationToken).ConfigureAwait(false);
        await StoreAsync(database, key, value).ConfigureAwait(false);

        return value;
    }

    /// <summary>
    /// Single-flight: só uma requisição por chave vai à origem; as outras esperam no
    /// semáforo e, ao entrar, encontram o cache já preenchido.
    /// </summary>
    public async Task<string> GetSingleFlightAsync(string key, CancellationToken cancellationToken)
    {
        IDatabase database = _redis.GetDatabase();
        RedisValue cached = await database.StringGetAsync(key).ConfigureAwait(false);

        if (cached.HasValue)
        {
            return Deserialize(cached!).Value;
        }

        SemaphoreSlim gate = _keyGates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Segunda checagem, obrigatoria: quem estava na fila do semaforo chega aqui
            // depois de outro ja ter populado o cache. Sem esta linha, o single-flight
            // vira uma fila de chamadas a origem, uma por requisicao — mais lento que o
            // ingenuo, e igualmente destrutivo.
            cached = await database.StringGetAsync(key).ConfigureAwait(false);
            if (cached.HasValue)
            {
                return Deserialize(cached!).Value;
            }

            string value = await _origin.LoadAsync(key, cancellationToken).ConfigureAwait(false);
            await StoreAsync(database, key, value).ConfigureAwait(false);

            return value;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>
    /// Stale-while-revalidate: passado o prazo curto, devolve o valor velho na hora e
    /// dispara a atualização em segundo plano. Ninguém espera pela origem — ao custo de
    /// servir, por alguns instantes, um dado desatualizado.
    /// </summary>
    public async Task<StaleResult> GetStaleWhileRevalidateAsync(string key, CancellationToken cancellationToken)
    {
        IDatabase database = _redis.GetDatabase();
        RedisValue cached = await database.StringGetAsync(key).ConfigureAwait(false);

        if (!cached.HasValue)
        {
            // Sem nada em cache nao ha o que servir: aqui a espera e inevitavel, e vale
            // usar single-flight para nao multiplicar a chamada.
            string fresh = await GetSingleFlightAsync(key, cancellationToken).ConfigureAwait(false);

            return new StaleResult(fresh, false, false);
        }

        CachedEntry entry = Deserialize(cached!);

        if (DateTimeOffset.UtcNow < entry.SoftExpiresAt)
        {
            return new StaleResult(entry.Value, false, false);
        }

        // Valor velho, porem utilizavel. Dispara UMA revalidacao e devolve o que tem.
        bool startedRefresh = _refreshing.TryAdd(key, 0);

        if (startedRefresh)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    string fresh = await _origin.LoadAsync(key, CancellationToken.None).ConfigureAwait(false);
                    await StoreAsync(_redis.GetDatabase(), key, fresh).ConfigureAwait(false);

                    _logger.LogInformation("Revalidacao de {Chave} concluida em segundo plano.", key);
                }
                catch (Exception ex)
                {
                    // Falha na revalidacao nao pode derrubar nada: o valor velho continua
                    // sendo servido ate o TTL duro expirar.
                    _logger.LogError(ex, "Revalidacao de {Chave} falhou; o valor antigo segue valendo.", key);
                }
                finally
                {
                    _refreshing.TryRemove(key, out _);
                }
            }, CancellationToken.None);
        }

        return new StaleResult(entry.Value, true, startedRefresh);
    }

    public async Task ClearAsync(string key)
    {
        await _redis.GetDatabase().KeyDeleteAsync(key).ConfigureAwait(false);
    }

    private static async Task StoreAsync(IDatabase database, string key, string value)
    {
        CachedEntry entry = new CachedEntry
        {
            Value = value,
            SoftExpiresAt = DateTimeOffset.UtcNow.Add(SoftTtl)
        };

        // TTL duro com jitter: o Redis apaga a chave neste prazo.
        await database.StringSetAsync(
            key,
            JsonSerializer.Serialize(entry, JsonOptions),
            TtlJitter.Apply(BaseTtl)).ConfigureAwait(false);
    }

    private static CachedEntry Deserialize(string payload)
    {
        return JsonSerializer.Deserialize<CachedEntry>(payload, JsonOptions) ?? new CachedEntry();
    }
}

public sealed record StaleResult(string Value, bool WasStale, bool TriggeredRefresh);
