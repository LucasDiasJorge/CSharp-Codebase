using ParallelDataProcessingDemo.Models;
using ParallelDataProcessingDemo.Workloads;

namespace ParallelDataProcessingDemo.Strategies;

/// <summary>
/// Linha de base: um item por vez, aguardando cada um antes de comecar o proximo.
/// E a referencia contra a qual o ganho das demais estrategias e calculado.
/// </summary>
public sealed class SequentialStrategy : IProcessingStrategy
{
    public string Name => "Sequencial (await em laco)";

    public async Task ExecuteAsync(
        IReadOnlyList<WorkItem> items,
        IWorkload workload,
        ConcurrencyTracker tracker,
        CancellationToken cancellationToken)
    {
        foreach (WorkItem item in items)
        {
            await workload.ProcessAsync(item, tracker, cancellationToken);
        }
    }
}
