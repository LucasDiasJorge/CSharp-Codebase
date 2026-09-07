using ParallelDataProcessingDemo.Models;

namespace ParallelDataProcessingDemo.Workloads;

/// <summary>
/// Carga de I/O: a thread nao trabalha, apenas espera. Enquanto o <see cref="Task.Delay(TimeSpan, CancellationToken)"/>
/// esta pendente a thread volta ao pool, entao centenas de itens podem estar "em andamento"
/// com pouquissimas threads — o limite util aqui e o servico remoto, nao o processador.
/// </summary>
public sealed class IoBoundWorkload : IWorkload
{
    private readonly TimeSpan latency;

    public IoBoundWorkload(TimeSpan latency)
    {
        this.latency = latency;
    }

    public string Name => $"I/O ({latency.TotalMilliseconds:F0}ms por item)";

    public async ValueTask ProcessAsync(
        WorkItem item,
        ConcurrencyTracker tracker,
        CancellationToken cancellationToken)
    {
        tracker.Enter();
        try
        {
            await Task.Delay(latency, cancellationToken);
            tracker.AddResult(item.Seed);
        }
        finally
        {
            tracker.Exit();
        }
    }
}
