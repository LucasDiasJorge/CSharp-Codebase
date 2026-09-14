using System.Collections.Concurrent;

namespace HealthChecksApi.Dependencies;

/// <summary>
/// Guarda o estado das dependências simuladas e finge o custo de uma checagem. Permite
/// virar cada dependência por HTTP e observar o efeito nas sondas — sem isso, o exemplo
/// só mostraria o caminho feliz.
/// </summary>
public sealed class DependencySimulator
{
    public const string Database = "database";
    public const string MessageBroker = "broker";
    public const string Cache = "cache";

    private static readonly TimeSpan SlowLatency = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan HangingLatency = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan NormalLatency = TimeSpan.FromMilliseconds(15);

    private readonly ConcurrentDictionary<string, DependencyState> _states = new ConcurrentDictionary<string, DependencyState>
    {
        [Database] = DependencyState.Healthy,
        [MessageBroker] = DependencyState.Healthy,
        [Cache] = DependencyState.Healthy
    };

    private readonly ILogger<DependencySimulator> _logger;

    public DependencySimulator(ILogger<DependencySimulator> logger)
    {
        _logger = logger;
    }

    public IReadOnlyDictionary<string, DependencyState> States => _states;

    public DependencyState GetState(string dependency)
    {
        return _states.TryGetValue(dependency, out DependencyState state) ? state : DependencyState.Healthy;
    }

    public bool TrySetState(string dependency, DependencyState state)
    {
        if (!_states.ContainsKey(dependency))
        {
            return false;
        }

        _states[dependency] = state;
        _logger.LogWarning("Dependencia {Dependencia} passou para o estado {Estado}.", dependency, state);

        return true;
    }

    /// <summary>
    /// Simula o custo de consultar a dependência. O <paramref name="cancellationToken"/>
    /// é o do próprio health check — é ele que o timeout configurado dispara.
    /// </summary>
    public async Task<TimeSpan> ProbeAsync(string dependency, CancellationToken cancellationToken)
    {
        DependencyState state = GetState(dependency);

        if (state == DependencyState.Down)
        {
            throw new InvalidOperationException($"Nao foi possivel conectar em '{dependency}'.");
        }

        TimeSpan latency = state switch
        {
            DependencyState.Slow => SlowLatency,
            DependencyState.Hanging => HangingLatency,
            _ => NormalLatency
        };
        await Task.Delay(latency, cancellationToken).ConfigureAwait(false);

        return latency;
    }
}
