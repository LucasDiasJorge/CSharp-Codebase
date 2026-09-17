namespace RedisDistributedLockDemo.Work;

/// <summary>
/// O recurso que o lock deveria proteger. Conta quantos trabalhadores estão dentro da
/// seção crítica ao mesmo tempo — é assim que a falha do lock deixa de ser teoria e
/// vira um número.
/// </summary>
public sealed class ProtectedResource
{
    private readonly object _gate = new object();
    private readonly List<string> _log = new List<string>();
    private readonly ILogger<ProtectedResource> _logger;

    private int _currentHolders;
    private int _maxConcurrentHolders;
    private int _violations;

    /// <summary>
    /// Maior fencing token já aceito. Escritas com número menor são rejeitadas — é o
    /// que salva a integridade quando dois processos acham que têm o lock.
    /// </summary>
    private long _lastAcceptedFence;

    public ProtectedResource(ILogger<ProtectedResource> logger)
    {
        _logger = logger;
    }

    public int MaxConcurrentHolders => Volatile.Read(ref _maxConcurrentHolders);

    /// <summary>Quantas vezes mais de um trabalhador esteve na seção crítica.</summary>
    public int Violations => Volatile.Read(ref _violations);

    public long LastAcceptedFence => Interlocked.Read(ref _lastAcceptedFence);

    public IReadOnlyList<string> Log
    {
        get
        {
            lock (_gate)
            {
                return _log.ToArray();
            }
        }
    }

    public void Enter(string owner)
    {
        int holders = Interlocked.Increment(ref _currentHolders);

        lock (_gate)
        {
            if (holders > _maxConcurrentHolders)
            {
                _maxConcurrentHolders = holders;
            }

            if (holders > 1)
            {
                Interlocked.Increment(ref _violations);
                _log.Add($"VIOLACAO: {owner} entrou com {holders} trabalhadores na secao critica");
                _logger.LogError("Exclusao mutua violada: {Quantidade} trabalhadores dentro ao mesmo tempo.", holders);
            }
            else
            {
                _log.Add($"{owner} entrou na secao critica");
            }
        }
    }

    public void Exit(string owner)
    {
        Interlocked.Decrement(ref _currentHolders);

        lock (_gate)
        {
            _log.Add($"{owner} saiu da secao critica");
        }
    }

    /// <summary>
    /// Escrita protegida por fencing token. Mesmo com dois processos convencidos de que
    /// têm o lock, apenas o mais recente consegue escrever.
    /// </summary>
    public bool TryWrite(string owner, long fencingToken, string value)
    {
        lock (_gate)
        {
            if (fencingToken < _lastAcceptedFence)
            {
                _log.Add($"REJEITADO: {owner} tentou escrever com fencing {fencingToken}, menor que {_lastAcceptedFence}");
                _logger.LogWarning(
                    "Escrita de {Dono} rejeitada: fencing {Recebido} < {Aceito}.",
                    owner,
                    fencingToken,
                    _lastAcceptedFence);

                return false;
            }

            _lastAcceptedFence = fencingToken;
            _log.Add($"{owner} escreveu '{value}' com fencing {fencingToken}");

            return true;
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            _log.Clear();
            _currentHolders = 0;
            _maxConcurrentHolders = 0;
            _violations = 0;
            _lastAcceptedFence = 0;
        }
    }
}
