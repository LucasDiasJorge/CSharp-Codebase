using System.Collections.Concurrent;

namespace ApiGatewayAggregationDemo.Downstream;

/// <summary>Como cada serviço de trás deve se comportar na próxima chamada.</summary>
public enum DownstreamBehavior
{
    /// <summary>Responde dentro da latência normal.</summary>
    Normal,

    /// <summary>Responde, mas depois do timeout do gateway.</summary>
    Slow,

    /// <summary>Responde 500.</summary>
    Failing
}

/// <summary>
/// Controla o comportamento dos serviços simulados. Existe para que cada modo de falha
/// possa ser disparado por uma requisição, em vez de descrito no README.
/// </summary>
public sealed class DownstreamSimulator
{
    public const string Profile = "profile";
    public const string Orders = "orders";
    public const string Recommendations = "recommendations";

    /// <summary>Latência normal de cada serviço, diferente de propósito.</summary>
    private static readonly IReadOnlyDictionary<string, int> NormalLatencyMs = new Dictionary<string, int>
    {
        [Profile] = 120,
        [Orders] = 400,
        [Recommendations] = 250
    };

    private const int SlowLatencyMs = 3000;

    private readonly ConcurrentDictionary<string, DownstreamBehavior> _behaviors = new ConcurrentDictionary<string, DownstreamBehavior>
    {
        [Profile] = DownstreamBehavior.Normal,
        [Orders] = DownstreamBehavior.Normal,
        [Recommendations] = DownstreamBehavior.Normal
    };

    private readonly ConcurrentDictionary<string, List<string>> _receivedCorrelationIds = new ConcurrentDictionary<string, List<string>>();

    public IReadOnlyDictionary<string, DownstreamBehavior> Behaviors => _behaviors;

    public bool TrySetBehavior(string service, DownstreamBehavior behavior)
    {
        if (!_behaviors.ContainsKey(service))
        {
            return false;
        }

        _behaviors[service] = behavior;

        return true;
    }

    public void Reset()
    {
        foreach (string service in _behaviors.Keys)
        {
            _behaviors[service] = DownstreamBehavior.Normal;
        }

        _receivedCorrelationIds.Clear();
    }

    /// <summary>
    /// Registra o correlation id que chegou. É a prova de que o identificador
    /// atravessou o gateway até cada serviço.
    /// </summary>
    public void RecordCorrelation(string service, string? correlationId)
    {
        _receivedCorrelationIds.GetOrAdd(service, _ => new List<string>())
            .Add(correlationId ?? "(ausente)");
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> ReceivedCorrelationIds
    {
        get
        {
            Dictionary<string, IReadOnlyList<string>> snapshot = new Dictionary<string, IReadOnlyList<string>>();
            foreach (KeyValuePair<string, List<string>> pair in _receivedCorrelationIds)
            {
                snapshot[pair.Key] = pair.Value.ToArray();
            }

            return snapshot;
        }
    }

    /// <summary>Executa o atraso e decide se a chamada falha.</summary>
    public async Task<bool> SimulateAsync(string service, CancellationToken cancellationToken)
    {
        DownstreamBehavior behavior = _behaviors.TryGetValue(service, out DownstreamBehavior value) ? value : DownstreamBehavior.Normal;

        int latency = behavior == DownstreamBehavior.Slow ? SlowLatencyMs : NormalLatencyMs[service];
        await Task.Delay(latency, cancellationToken).ConfigureAwait(false);

        return behavior != DownstreamBehavior.Failing;
    }
}
