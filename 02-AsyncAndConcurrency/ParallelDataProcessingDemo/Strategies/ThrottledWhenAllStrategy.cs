using ParallelDataProcessingDemo.Models;
using ParallelDataProcessingDemo.Workloads;

namespace ParallelDataProcessingDemo.Strategies;

/// <summary>
/// <c>Task.WhenAll</c> com teto de concorrencia imposto por <see cref="SemaphoreSlim"/>.
/// Era a forma usual de limitar paralelismo antes de <c>Parallel.ForEachAsync</c>; continua
/// util quando o limite precisa ser compartilhado entre lacos diferentes.
/// </summary>
public sealed class ThrottledWhenAllStrategy : IProcessingStrategy
{
    private readonly int maxConcurrency;

    public ThrottledWhenAllStrategy(int maxConcurrency)
    {
        this.maxConcurrency = maxConcurrency;
    }

    public string Name => $"WhenAll + SemaphoreSlim({maxConcurrency})";

    public async Task ExecuteAsync(
        IReadOnlyList<WorkItem> items,
        IWorkload workload,
        ConcurrencyTracker tracker,
        CancellationToken cancellationToken)
    {
        using SemaphoreSlim gate = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        List<Task> pendingTasks = new List<Task>(items.Count);

        foreach (WorkItem item in items)
        {
            pendingTasks.Add(ProcessWithGateAsync(item, workload, tracker, gate, cancellationToken));
        }

        await Task.WhenAll(pendingTasks);
    }

    private static async Task ProcessWithGateAsync(
        WorkItem item,
        IWorkload workload,
        ConcurrencyTracker tracker,
        SemaphoreSlim gate,
        CancellationToken cancellationToken)
    {
        // O Wait acontece dentro da task, entao o laco de criacao nao bloqueia.
        await gate.WaitAsync(cancellationToken);
        try
        {
            await workload.ProcessAsync(item, tracker, cancellationToken);
        }
        finally
        {
            // Sem este Release em finally, uma falha trava o lote inteiro.
            gate.Release();
        }
    }
}
