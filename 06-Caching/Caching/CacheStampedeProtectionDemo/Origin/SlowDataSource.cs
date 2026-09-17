namespace CacheStampedeProtectionDemo.Origin;

/// <summary>
/// A fonte de dados que o cache existe para proteger. Conta cada chamada — é esse
/// número, e não o tempo de resposta, que mostra se a proteção contra stampede
/// funcionou.
/// </summary>
public sealed class SlowDataSource
{
    /// <summary>
    /// Latência da origem. É ela que abre a janela do stampede: quanto mais demorada a
    /// consulta, mais requisições chegam antes de a primeira terminar e popular o cache.
    /// </summary>
    public static readonly TimeSpan Latency = TimeSpan.FromMilliseconds(400);

    private readonly ILogger<SlowDataSource> _logger;
    private int _callCount;

    public SlowDataSource(ILogger<SlowDataSource> logger)
    {
        _logger = logger;
    }

    public int CallCount => Volatile.Read(ref _callCount);

    public async Task<string> LoadAsync(string key, CancellationToken cancellationToken)
    {
        int call = Interlocked.Increment(ref _callCount);

        _logger.LogWarning("Origem consultada para {Chave} (chamada #{Numero}).", key, call);

        await Task.Delay(Latency, cancellationToken).ConfigureAwait(false);

        return $"valor-de-{key}@{DateTimeOffset.UtcNow:HH:mm:ss.fff}";
    }

    public void ResetCounter()
    {
        Interlocked.Exchange(ref _callCount, 0);
    }
}
