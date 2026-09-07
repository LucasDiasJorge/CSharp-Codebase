using ParallelDataProcessingDemo.Models;

namespace ParallelDataProcessingDemo.Workloads;

/// <summary>
/// Carga de CPU: a thread trabalha o tempo todo, nao ha espera para aproveitar.
///
/// O parametro <c>offloadToThreadPool</c> existe para expor uma armadilha comum. Com ele
/// desligado o calculo roda direto no corpo do metodo, que retorna uma task ja concluida;
/// nesse caso <c>Task.WhenAll</c> nao paraleliza nada, porque nao sobra nada para aguardar.
/// Marcar o metodo como <c>async</c> nao cria concorrencia — quem cria e o offload explicito.
/// </summary>
public sealed class CpuBoundWorkload : IWorkload
{
    private const int IterationsPerItem = 12_000_000;

    private readonly bool offloadToThreadPool;

    public CpuBoundWorkload(bool offloadToThreadPool)
    {
        this.offloadToThreadPool = offloadToThreadPool;
    }

    public string Name => offloadToThreadPool
        ? "CPU (com Task.Run)"
        : "CPU (sem offload)";

    public ValueTask ProcessAsync(
        WorkItem item,
        ConcurrencyTracker tracker,
        CancellationToken cancellationToken)
    {
        if (!offloadToThreadPool)
        {
            // Roda inteiramente antes de devolver o controle: a task ja nasce concluida.
            RunComputation(item, tracker);
            return ValueTask.CompletedTask;
        }

        return new ValueTask(Task.Run(() => RunComputation(item, tracker), cancellationToken));
    }

    private static void RunComputation(WorkItem item, ConcurrencyTracker tracker)
    {
        tracker.Enter();
        try
        {
            long accumulator = item.Seed;
            for (int iteration = 1; iteration <= IterationsPerItem; iteration++)
            {
                // Cada passo depende do anterior, entao o compilador nao pode descartar o laco.
                accumulator = ((accumulator * 31) + iteration) % 1_000_003;
            }

            tracker.AddResult(accumulator);
        }
        finally
        {
            tracker.Exit();
        }
    }
}
