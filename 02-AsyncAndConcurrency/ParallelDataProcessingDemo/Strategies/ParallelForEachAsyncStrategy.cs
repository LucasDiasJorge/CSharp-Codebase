using ParallelDataProcessingDemo.Models;
using ParallelDataProcessingDemo.Workloads;

namespace ParallelDataProcessingDemo.Strategies;

/// <summary>
/// <c>Parallel.ForEachAsync</c>: laco assincrono com grau de paralelismo declarado.
///
/// Nao confundir com <c>Parallel.ForEach</c>, que e sincrono e nao sabe aguardar uma task —
/// passar um delegate <c>async</c> para ele produz fire-and-forget silencioso.
/// </summary>
public sealed class ParallelForEachAsyncStrategy : IProcessingStrategy
{
    private readonly int maxDegreeOfParallelism;

    public ParallelForEachAsyncStrategy(int maxDegreeOfParallelism)
    {
        this.maxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    public string Name => $"Parallel.ForEachAsync(DOP={maxDegreeOfParallelism})";

    public Task ExecuteAsync(
        IReadOnlyList<WorkItem> items,
        IWorkload workload,
        ConcurrencyTracker tracker,
        CancellationToken cancellationToken)
    {
        ParallelOptions options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        return Parallel.ForEachAsync(
            items,
            options,
            (WorkItem item, CancellationToken itemToken) => workload.ProcessAsync(item, tracker, itemToken));
    }
}
