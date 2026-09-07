using ParallelDataProcessingDemo.Models;
using ParallelDataProcessingDemo.Workloads;

namespace ParallelDataProcessingDemo.Strategies;

/// <summary>
/// Forma de percorrer o lote. Todas as implementacoes fazem exatamente o mesmo trabalho;
/// mudam apenas quantos itens ficam em andamento ao mesmo tempo.
/// </summary>
public interface IProcessingStrategy
{
    string Name { get; }

    Task ExecuteAsync(
        IReadOnlyList<WorkItem> items,
        IWorkload workload,
        ConcurrencyTracker tracker,
        CancellationToken cancellationToken);
}
