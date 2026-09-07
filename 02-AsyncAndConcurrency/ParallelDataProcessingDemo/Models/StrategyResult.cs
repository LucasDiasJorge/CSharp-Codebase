namespace ParallelDataProcessingDemo.Models;

/// <summary>
/// Medicao de uma estrategia sobre um lote: quanto demorou, quantas execucoes chegaram a
/// coexistir e um checksum que prova que todos os itens foram realmente processados.
/// </summary>
public sealed class StrategyResult
{
    public StrategyResult(
        string strategyName,
        int itemCount,
        TimeSpan elapsed,
        int maxObservedConcurrency,
        long checksum)
    {
        StrategyName = strategyName;
        ItemCount = itemCount;
        Elapsed = elapsed;
        MaxObservedConcurrency = maxObservedConcurrency;
        Checksum = checksum;
    }

    public string StrategyName { get; }

    public int ItemCount { get; }

    public TimeSpan Elapsed { get; }

    /// <summary>Pico de execucoes simultaneas medido durante a passagem.</summary>
    public int MaxObservedConcurrency { get; }

    /// <summary>Soma dos resultados; igual entre estrategias quando todas fizeram o mesmo trabalho.</summary>
    public long Checksum { get; }

    /// <summary>Ganho em relacao a execucao sequencial do mesmo lote.</summary>
    public double SpeedupOver(StrategyResult baseline)
    {
        if (Elapsed <= TimeSpan.Zero)
        {
            return 0.0;
        }

        return baseline.Elapsed.TotalMilliseconds / Elapsed.TotalMilliseconds;
    }

    public string Describe(StrategyResult baseline)
    {
        return $"{StrategyName,-34} {Elapsed.TotalMilliseconds,7:F0}ms  " +
            $"pico {MaxObservedConcurrency,2}  ganho {SpeedupOver(baseline),5:F2}x";
    }
}
