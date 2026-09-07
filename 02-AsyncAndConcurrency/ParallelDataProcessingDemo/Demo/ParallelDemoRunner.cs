using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ParallelDataProcessingDemo.Models;
using ParallelDataProcessingDemo.Strategies;
using ParallelDataProcessingDemo.Workloads;

namespace ParallelDataProcessingDemo.Demo;

/// <summary>
/// Roda as mesmas estrategias sobre cargas diferentes e imprime a comparacao.
/// A conclusao a ser observada e que a ordem das estrategias muda conforme a carga.
/// </summary>
public sealed class ParallelDemoRunner
{
    private const int IoItemCount = 12;
    private const int CpuItemCount = 8;

    private static readonly TimeSpan IoLatency = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// O provider de console do <c>Microsoft.Extensions.Logging</c> grava em uma fila processada
    /// por thread propria. Sem esta pausa, a tabela escrita direto no <see cref="Console"/>
    /// apareceria antes dos logs das medicoes que ela resume.
    /// </summary>
    private static readonly TimeSpan LogFlushDelay = TimeSpan.FromMilliseconds(100);

    private readonly ILogger<ParallelDemoRunner> logger;
    private readonly int processorCount;

    public ParallelDemoRunner(ILogger<ParallelDemoRunner> logger)
    {
        this.logger = logger;
        processorCount = Environment.ProcessorCount;
    }

    public async Task RunAllAsync()
    {
        Console.WriteLine($"Processadores logicos disponiveis: {processorCount}");

        await RunIoBoundComparisonAsync();
        await RunCpuBoundComparisonAsync();
        await RunCpuWithoutOffloadAsync();
        await RunDegreeOfParallelismSweepAsync();
    }

    /// <summary>
    /// Cenario 1: carga de I/O. A thread fica esperando, entao vale ter muitos itens em voo —
    /// o paralelismo util passa longe do numero de nucleos.
    /// </summary>
    private async Task RunIoBoundComparisonAsync()
    {
        PrintHeader($"1. Carga de I/O — {IoItemCount} itens de {IoLatency.TotalMilliseconds:F0}ms");

        IWorkload workload = new IoBoundWorkload(IoLatency);
        IReadOnlyList<WorkItem> items = CreateItems(IoItemCount);

        List<StrategyResult> results = new List<StrategyResult>
        {
            await MeasureAsync(new SequentialStrategy(), items, workload),
            await MeasureAsync(new WhenAllStrategy(), items, workload),
            await MeasureAsync(new ThrottledWhenAllStrategy(4), items, workload),
            await MeasureAsync(new ParallelForEachAsyncStrategy(4), items, workload),
            await MeasureAsync(new ParallelForEachAsyncStrategy(IoItemCount), items, workload)
        };

        await PrintTableAsync(results);
    }

    /// <summary>
    /// Cenario 2: carga de CPU com offload explicito. Aqui o teto e fisico: passar do numero
    /// de nucleos nao acelera, so aumenta troca de contexto.
    /// </summary>
    private async Task RunCpuBoundComparisonAsync()
    {
        PrintHeader($"2. Carga de CPU com Task.Run — {CpuItemCount} itens");

        IWorkload workload = new CpuBoundWorkload(offloadToThreadPool: true);
        IReadOnlyList<WorkItem> items = CreateItems(CpuItemCount);

        List<StrategyResult> results = new List<StrategyResult>
        {
            await MeasureAsync(new SequentialStrategy(), items, workload),
            await MeasureAsync(new WhenAllStrategy(), items, workload),
            await MeasureAsync(new ParallelForEachAsyncStrategy(2), items, workload),
            await MeasureAsync(new ParallelForEachAsyncStrategy(processorCount), items, workload)
        };

        await PrintTableAsync(results);
    }

    /// <summary>
    /// Cenario 3: a mesma carga de CPU sem <c>Task.Run</c>. O contraste e o ponto:
    /// <c>Task.WhenAll</c> nao tem o que aguardar e degenera em sequencial, enquanto
    /// <c>Parallel.ForEachAsync</c> continua paralelizando, porque ele mesmo distribui as
    /// invocacoes entre varios workers.
    /// </summary>
    private async Task RunCpuWithoutOffloadAsync()
    {
        PrintHeader($"3. Carga de CPU sem offload — {CpuItemCount} itens");

        IWorkload workload = new CpuBoundWorkload(offloadToThreadPool: false);
        IReadOnlyList<WorkItem> items = CreateItems(CpuItemCount);

        List<StrategyResult> results = new List<StrategyResult>
        {
            await MeasureAsync(new SequentialStrategy(), items, workload),
            await MeasureAsync(new WhenAllStrategy(), items, workload),
            await MeasureAsync(new ParallelForEachAsyncStrategy(processorCount), items, workload)
        };

        await PrintTableAsync(results);
    }

    /// <summary>
    /// Cenario 4: mesma carga de I/O com o grau de paralelismo subindo. O ganho cresce ate
    /// o lote acabar; depois disso, aumentar o limite nao muda mais nada.
    /// </summary>
    private async Task RunDegreeOfParallelismSweepAsync()
    {
        PrintHeader($"4. Grau de paralelismo sobre I/O — {IoItemCount} itens");

        IWorkload workload = new IoBoundWorkload(IoLatency);
        IReadOnlyList<WorkItem> items = CreateItems(IoItemCount);
        int[] degrees = new int[] { 1, 2, 4, 8, 16 };

        List<StrategyResult> results = new List<StrategyResult>(degrees.Length);
        foreach (int degree in degrees)
        {
            results.Add(await MeasureAsync(new ParallelForEachAsyncStrategy(degree), items, workload));
        }

        await PrintTableAsync(results);
    }

    private async Task<StrategyResult> MeasureAsync(
        IProcessingStrategy strategy,
        IReadOnlyList<WorkItem> items,
        IWorkload workload)
    {
        ConcurrencyTracker tracker = new ConcurrencyTracker();
        Stopwatch stopwatch = Stopwatch.StartNew();

        await strategy.ExecuteAsync(items, workload, tracker, CancellationToken.None);

        stopwatch.Stop();

        logger.LogInformation(
            "{Workload} | {Strategy} | {Elapsed}ms | pico de concorrencia {Peak}",
            workload.Name,
            strategy.Name,
            stopwatch.Elapsed.TotalMilliseconds,
            tracker.MaxObserved);

        return new StrategyResult(
            strategy.Name,
            items.Count,
            stopwatch.Elapsed,
            tracker.MaxObserved,
            tracker.Checksum);
    }

    private static IReadOnlyList<WorkItem> CreateItems(int count)
    {
        List<WorkItem> items = new List<WorkItem>(count);
        for (int index = 1; index <= count; index++)
        {
            items.Add(new WorkItem(index, index * 7));
        }

        return items;
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static async Task PrintTableAsync(IReadOnlyList<StrategyResult> results)
    {
        await Task.Delay(LogFlushDelay);

        StrategyResult baseline = results[0];
        foreach (StrategyResult result in results)
        {
            Console.WriteLine($">> {result.Describe(baseline)}");
        }

        Console.WriteLine($"   checksum: {DescribeChecksums(results)}");
    }

    /// <summary>
    /// Todas as estrategias fazem o mesmo trabalho, entao os checksums tem que coincidir.
    /// Um valor divergente indicaria item perdido ou processado duas vezes.
    /// </summary>
    private static string DescribeChecksums(IReadOnlyList<StrategyResult> results)
    {
        long firstChecksum = results[0].Checksum;
        foreach (StrategyResult result in results)
        {
            if (result.Checksum != firstChecksum)
            {
                return "DIVERGENTE — alguma estrategia perdeu ou repetiu itens";
            }
        }

        return $"{firstChecksum} (igual em todas)";
    }
}
