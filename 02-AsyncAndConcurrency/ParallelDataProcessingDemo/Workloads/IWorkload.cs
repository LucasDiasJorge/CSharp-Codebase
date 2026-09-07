using ParallelDataProcessingDemo.Models;

namespace ParallelDataProcessingDemo.Workloads;

/// <summary>
/// Trabalho aplicado a cada item. Trocar a implementacao — I/O ou CPU — mantendo as mesmas
/// estrategias e o que revela que nao existe "estrategia mais rapida" em abstrato.
/// </summary>
public interface IWorkload
{
    string Name { get; }

    ValueTask ProcessAsync(WorkItem item, ConcurrencyTracker tracker, CancellationToken cancellationToken);
}
