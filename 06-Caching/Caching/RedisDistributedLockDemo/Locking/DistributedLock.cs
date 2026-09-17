using System.Security.Cryptography;
using StackExchange.Redis;

namespace RedisDistributedLockDemo.Locking;

/// <summary>
/// Lock distribuído sobre Redis. As três operações têm exigências diferentes, e é nelas
/// que mora a diferença entre um lock que funciona e um que parece funcionar.
/// </summary>
public sealed class DistributedLock
{
    /// <summary>
    /// Libera apenas se o token conferir. Precisa ser script Lua porque GET e DEL como
    /// comandos separados não são atômicos: entre um e outro o lock pode expirar e ser
    /// adquirido por outro processo, e o DEL apagaria o lock ALHEIO.
    /// </summary>
    private const string ReleaseScript = @"
if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1])
else
    return 0
end";

    /// <summary>
    /// Renova apenas se ainda somos o dono. Um PEXPIRE cego estenderia o lock de outro
    /// processo, o que é pior do que não renovar.
    /// </summary>
    private const string RenewScript = @"
if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('PEXPIRE', KEYS[1], ARGV[2])
else
    return 0
end";

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<DistributedLock> _logger;

    public DistributedLock(IConnectionMultiplexer redis, ILogger<DistributedLock> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Tenta adquirir com <c>SET key token NX PX ttl</c> — uma única operação atômica.
    /// O token aleatório é o que identifica o dono; sem ele não há como distinguir "meu
    /// lock" de "o lock de quem veio depois de mim".
    /// </summary>
    public async Task<LockHandle?> TryAcquireAsync(string resource, TimeSpan ttl, string owner)
    {
        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        IDatabase database = _redis.GetDatabase();

        bool acquired = await database.StringSetAsync(
            Key(resource),
            token,
            ttl,
            when: When.NotExists).ConfigureAwait(false);

        if (!acquired)
        {
            return null;
        }

        // Fencing token: numero monotonico entregue junto com o lock. E o unico
        // mecanismo que protege de verdade quando o lock expira sem o dono perceber —
        // o recurso protegido rejeita qualquer escrita com numero menor que o ultimo
        // que ja aceitou. Sem isso, lock distribuido e otimizacao, nao garantia.
        long fencingToken = await database.StringIncrementAsync(FenceKey(resource)).ConfigureAwait(false);

        _logger.LogInformation(
            "{Dono} adquiriu {Recurso} (fencing {Fence}, ttl {Ttl}ms).",
            owner,
            resource,
            fencingToken,
            ttl.TotalMilliseconds);

        return new LockHandle(resource, token, owner, fencingToken, ttl);
    }

    /// <summary>Renova o lease. Devolve false se já perdemos o lock.</summary>
    public async Task<bool> TryRenewAsync(LockHandle handle)
    {
        RedisResult result = await _redis.GetDatabase().ScriptEvaluateAsync(
            RenewScript,
            [Key(handle.Resource)],
            [handle.Token, (long)handle.Ttl.TotalMilliseconds]).ConfigureAwait(false);

        bool renewed = (long)result == 1;

        if (!renewed)
        {
            _logger.LogWarning("{Dono} tentou renovar {Recurso}, mas ja nao e o dono.", handle.Owner, handle.Resource);
        }

        return renewed;
    }

    /// <summary>Libera com verificação de dono.</summary>
    public async Task<bool> ReleaseAsync(LockHandle handle)
    {
        RedisResult result = await _redis.GetDatabase().ScriptEvaluateAsync(
            ReleaseScript,
            [Key(handle.Resource)],
            [handle.Token]).ConfigureAwait(false);

        bool released = (long)result == 1;

        if (!released)
        {
            _logger.LogWarning(
                "{Dono} tentou liberar {Recurso} sem ser o dono — o lease havia expirado e outro processo assumiu.",
                handle.Owner,
                handle.Resource);
        }

        return released;
    }

    /// <summary>
    /// Liberação ingênua: apaga sem conferir o token. Existe para demonstrar o estrago —
    /// se o lease expirou e outro processo pegou o lock, este DEL apaga o lock dele.
    /// </summary>
    public async Task<bool> ReleaseUnsafeAsync(string resource)
    {
        return await _redis.GetDatabase().KeyDeleteAsync(Key(resource)).ConfigureAwait(false);
    }

    public async Task<LockState> GetStateAsync(string resource)
    {
        IDatabase database = _redis.GetDatabase();
        RedisValue token = await database.StringGetAsync(Key(resource)).ConfigureAwait(false);
        TimeSpan? ttl = await database.KeyTimeToLiveAsync(Key(resource)).ConfigureAwait(false);

        return new LockState(token.HasValue, token.HasValue ? token!.ToString()[..8] : null, ttl?.TotalMilliseconds);
    }

    private static string Key(string resource) => $"lock:{resource}";

    private static string FenceKey(string resource) => $"lock:{resource}:fence";
}

/// <summary>Posse do lock: recurso, token secreto, dono e fencing token.</summary>
public sealed record LockHandle(string Resource, string Token, string Owner, long FencingToken, TimeSpan Ttl);

public sealed record LockState(bool Held, string? TokenPrefix, double? TtlMs);
