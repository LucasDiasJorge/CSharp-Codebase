namespace ChannelProducerConsumer.Models;

/// <summary>
/// Resumo de uma execucao produtor-consumidor: o que foi escrito, o que foi lido,
/// quanto tempo os produtores ficaram bloqueados esperando espaco no canal e se
/// a ordem de entrada sobreviveu ate a saida.
/// </summary>
public sealed class ChannelRunSummary
{
    public ChannelRunSummary(
        string scenarioName,
        int producedCount,
        int consumedCount,
        TimeSpan producerBlockedTime,
        TimeSpan totalElapsed,
        int maxObservedQueueDepth,
        bool orderPreserved)
    {
        ScenarioName = scenarioName;
        ProducedCount = producedCount;
        ConsumedCount = consumedCount;
        ProducerBlockedTime = producerBlockedTime;
        TotalElapsed = totalElapsed;
        MaxObservedQueueDepth = maxObservedQueueDepth;
        OrderPreserved = orderPreserved;
    }

    public string ScenarioName { get; }

    public int ProducedCount { get; }

    public int ConsumedCount { get; }

    /// <summary>Itens aceitos pelo produtor mas nunca entregues, por descarte do canal.</summary>
    public int DroppedCount => ProducedCount - ConsumedCount;

    /// <summary>Tempo somado que os produtores passaram dentro de <c>WriteAsync</c> esperando vaga.</summary>
    public TimeSpan ProducerBlockedTime { get; }

    public TimeSpan TotalElapsed { get; }

    /// <summary>Maior profundidade de fila observada; em canal bounded nunca passa da capacidade.</summary>
    public int MaxObservedQueueDepth { get; }

    public bool OrderPreserved { get; }

    public string Describe()
    {
        return $"{ScenarioName}: produzidos {ProducedCount}, consumidos {ConsumedCount}, " +
            $"descartados {DroppedCount} | espera do produtor {ProducerBlockedTime.TotalMilliseconds:F0}ms | " +
            $"fila maxima {MaxObservedQueueDepth} | total {TotalElapsed.TotalMilliseconds:F0}ms | " +
            $"ordem {(OrderPreserved ? "preservada" : "embaralhada")}";
    }
}
