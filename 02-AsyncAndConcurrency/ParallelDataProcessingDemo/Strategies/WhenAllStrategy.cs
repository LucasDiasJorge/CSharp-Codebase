using ParallelDataProcessingDemo.Models;
using ParallelDataProcessingDemo.Workloads;

namespace ParallelDataProcessingDemo.Strategies;

/// <summary>
/// Dispara tudo de uma vez e aguarda o conjunto. Otimo para I/O com poucos itens e
/// perigoso em lote grande: nao ha limite de quantas chamadas ficam em voo, o que pode
/// derrubar o servico do outro lado.
/// </summary>
public sealed class WhenAllStrategy : IProcessingStrategy
{
    public string Name => "Task.WhenAll (sem limite)";

    public Task ExecuteAsync(
        IReadOnlyList<WorkItem> items,
        IWorkload workload,
        ConcurrencyTracker tracker,
        CancellationToken cancellationToken)
    {
        List<Task> pendingTasks = new List<Task>(items.Count);

        foreach (WorkItem item in items)
        {
            // AsTask materializa a ValueTask: so assim ela pode ser guardada e aguardada depois.
            pendingTasks.Add(workload.ProcessAsync(item, tracker, cancellationToken).AsTask());
        }

        return Task.WhenAll(pendingTasks);
    }
}
