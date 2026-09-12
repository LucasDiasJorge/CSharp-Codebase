namespace AsyncLockingDemo.Models;

/// <summary>
/// Efeito de um cenário sobre a thread pool, medido durante a execução.
/// </summary>
public sealed class ThreadPoolSnapshot
{
    public ThreadPoolSnapshot(int baselineThreadCount, int peakThreadCount, int heartbeats, double maxHeartbeatGapMs)
    {
        BaselineThreadCount = baselineThreadCount;
        PeakThreadCount = peakThreadCount;
        Heartbeats = heartbeats;
        MaxHeartbeatGapMs = maxHeartbeatGapMs;
    }

    /// <summary>Threads da pool existentes antes de o cenário começar.</summary>
    public int BaselineThreadCount { get; }

    /// <summary>Maior número de threads da pool observado durante o cenário.</summary>
    public int PeakThreadCount { get; }

    /// <summary>Batidas concluídas pelo heartbeat agendado na thread pool.</summary>
    public int Heartbeats { get; }

    /// <summary>
    /// Maior intervalo entre duas batidas. Com espera bloqueante, o heartbeat fica
    /// sem thread para rodar e esse número dispara — é a assinatura da starvation.
    /// </summary>
    public double MaxHeartbeatGapMs { get; }

    public int InjectedThreads => PeakThreadCount - BaselineThreadCount;
}
