using System.Diagnostics;
using AsyncStreamsDemo.Models;
using AsyncStreamsDemo.Sources;
using Microsoft.Extensions.Logging;

namespace AsyncStreamsDemo.Demo;

/// <summary>
/// Executa os cenarios em sequencia: entrega progressiva, preguica na paginacao,
/// cancelamento correto e quebrado, composicao de operadores e re-enumeracao.
/// </summary>
public sealed class AsyncStreamsDemoRunner
{
    private const int StreamReadingCount = 6;
    private const int CancellableReadingCount = 8;
    private const int PageSize = 4;
    private const int TotalPages = 5;

    private static readonly TimeSpan ReadingInterval = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan PageLatency = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan CancellationDelay = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// O provider de console do <c>Microsoft.Extensions.Logging</c> grava em uma fila processada
    /// por thread propria. Sem esta pausa, o resumo escrito direto no <see cref="Console"/>
    /// apareceria antes dos logs do cenario que ele resume.
    /// </summary>
    private static readonly TimeSpan LogFlushDelay = TimeSpan.FromMilliseconds(100);

    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<AsyncStreamsDemoRunner> logger;

    public AsyncStreamsDemoRunner(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        logger = loggerFactory.CreateLogger<AsyncStreamsDemoRunner>();
    }

    public async Task RunAllAsync()
    {
        await RunProgressiveDeliveryAsync();
        await RunLazyPaginationAsync();
        await RunCancellationAsync();
        await RunLazyCompositionAsync();
        await RunReenumerationPitfallAsync();
    }

    /// <summary>
    /// Cenario 1: o mesmo trabalho entregue de duas formas. O tempo total e praticamente
    /// igual; o que muda e quando o primeiro item fica disponivel.
    /// </summary>
    private async Task RunProgressiveDeliveryAsync()
    {
        PrintHeader($"1. Entrega progressiva — {StreamReadingCount} leituras de {ReadingInterval.TotalMilliseconds:F0}ms");

        SensorFeed feed = CreateFeed();

        Stopwatch streamWatch = Stopwatch.StartNew();
        TimeSpan firstFromStream = TimeSpan.Zero;
        int streamCount = 0;

        await foreach (SensorReading reading in feed.StreamAsync(StreamReadingCount))
        {
            if (streamCount == 0)
            {
                firstFromStream = streamWatch.Elapsed;
            }

            streamCount++;
        }

        streamWatch.Stop();

        Stopwatch listWatch = Stopwatch.StartNew();
        IReadOnlyList<SensorReading> readings = await feed.CollectAllAsync(StreamReadingCount);
        TimeSpan firstFromList = listWatch.Elapsed;
        listWatch.Stop();

        await PrintResultAsync(
            $"await foreach: 1o item em {firstFromStream.TotalMilliseconds:F0}ms, total {streamWatch.Elapsed.TotalMilliseconds:F0}ms ({streamCount} itens)");
        PrintLine(
            $"Task<List<T>>: 1o item em {firstFromList.TotalMilliseconds:F0}ms, total {listWatch.Elapsed.TotalMilliseconds:F0}ms ({readings.Count} itens)");
    }

    /// <summary>
    /// Cenario 2: paginacao escondida atras de um stream de itens. Sair do laco com `break`
    /// evita as chamadas das paginas seguintes.
    /// </summary>
    private async Task RunLazyPaginationAsync()
    {
        PrintHeader($"2. Paginacao preguicosa — {TotalPages} paginas de {PageSize} itens");

        PagedCatalogClient client = CreateCatalogClient();

        int consumedAll = 0;
        await foreach (string item in client.StreamItemsAsync())
        {
            consumedAll++;
        }

        int pagesForFullScan = client.FetchedPageCount;

        client.ResetCounters();

        int consumedPartial = 0;
        await foreach (string item in client.StreamItemsAsync())
        {
            consumedPartial++;
            if (consumedPartial == 5)
            {
                // O break dispara DisposeAsync no iterador; as paginas 3 a 5 nunca sao buscadas.
                break;
            }
        }

        int pagesForPartialScan = client.FetchedPageCount;

        await PrintResultAsync(
            $"consumo completo: {consumedAll} itens, {pagesForFullScan} paginas buscadas");
        PrintLine(
            $"break no 5o item: {consumedPartial} itens, {pagesForPartialScan} paginas buscadas");
    }

    /// <summary>
    /// Cenario 3: o atributo <c>[EnumeratorCancellation]</c> e o que liga o token do consumidor
    /// ao parametro do iterador. Sem ele, <c>WithCancellation</c> nao tem efeito nenhum.
    /// </summary>
    private async Task RunCancellationAsync()
    {
        PrintHeader("3. Cancelamento com e sem [EnumeratorCancellation]");

        int correctCount = await ConsumeAnnotatedStreamAsync();
        int brokenCount = await ConsumeUnannotatedStreamAsync();

        await PrintResultAsync(
            $"com o atributo: {correctCount} de {CancellableReadingCount} leituras consumidas antes do cancelamento");
        PrintLine(
            $"sem o atributo: {brokenCount} de {CancellableReadingCount} leituras consumidas — o token foi ignorado");
    }

    /// <summary>
    /// Caminho correto: o parametro do iterador tem <c>[EnumeratorCancellation]</c>, entao o
    /// token entregue por <c>WithCancellation</c> chega ao <c>Task.Delay</c> la dentro e a
    /// enumeracao para na hora.
    /// </summary>
    private async Task<int> ConsumeAnnotatedStreamAsync()
    {
        SensorFeed feed = CreateFeed();
        using CancellationTokenSource cancellationSource = new CancellationTokenSource(CancellationDelay);
        int consumedCount = 0;

        try
        {
            await foreach (SensorReading reading in feed
                .StreamAsync(CancellableReadingCount)
                .WithCancellation(cancellationSource.Token))
            {
                consumedCount++;
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("consumo interrompido pelo cancelamento apos {Count} leituras", consumedCount);
        }

        return consumedCount;
    }

    /// <summary>
    /// Caminho quebrado: o laco de consumo e identico, letra por letra. A unica diferenca
    /// esta na fonte, cujo parametro nao foi anotado — e por isso o cancelamento nao acontece.
    /// </summary>
    private async Task<int> ConsumeUnannotatedStreamAsync()
    {
        SensorFeed feed = CreateFeed();
        using CancellationTokenSource cancellationSource = new CancellationTokenSource(CancellationDelay);
        int consumedCount = 0;

        try
        {
            await foreach (SensorReading reading in feed
                .StreamIgnoringCancellationAsync(CancellableReadingCount)
                .WithCancellation(cancellationSource.Token))
            {
                consumedCount++;
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("consumo interrompido pelo cancelamento apos {Count} leituras", consumedCount);
        }

        return consumedCount;
    }

    /// <summary>
    /// Cenario 4: pipeline de operadores preguiçosos. A fonte tem 20 leituras disponiveis,
    /// mas so produz o necessario para satisfazer o <c>TakeAsync</c>.
    /// </summary>
    private async Task RunLazyCompositionAsync()
    {
        PrintHeader("4. Composicao preguicosa — WhereAsync + SelectAsync + TakeAsync");

        SensorFeed feed = CreateFeed();
        feed.ResetCounters();

        List<string> collected = new List<string>();

        IAsyncEnumerable<string> pipeline = feed
            .StreamAsync(readingCount: 20)
            .WhereAsync((SensorReading reading) => reading.SequenceNumber % 2 == 0)
            .SelectAsync((SensorReading reading) => $"par-{reading.SequenceNumber:D2}")
            .TakeAsync(3);

        await foreach (string formatted in pipeline)
        {
            collected.Add(formatted);
        }

        await PrintResultAsync(
            $"pedidos 3 itens: {string.Join(", ", collected)}");
        PrintLine(
            $"a fonte produziu {feed.ProducedCount} de 20 leituras disponiveis");
    }

    /// <summary>
    /// Cenario 5: armadilha. Um <see cref="IAsyncEnumerable{T}"/> nao guarda resultado;
    /// enumerar duas vezes refaz todo o I/O.
    /// </summary>
    private async Task RunReenumerationPitfallAsync()
    {
        PrintHeader("5. Armadilha: re-enumeracao refaz o trabalho");

        PagedCatalogClient client = CreateCatalogClient();
        IAsyncEnumerable<string> stream = client.StreamItemsAsync();

        int firstPass = await CountAsync(stream);
        int secondPass = await CountAsync(stream);
        int pagesAfterTwoPasses = client.FetchedPageCount;

        client.ResetCounters();

        // Materializar uma vez e reutilizar a lista evita a segunda rodada de chamadas.
        List<string> materialized = new List<string>();
        await foreach (string item in client.StreamItemsAsync())
        {
            materialized.Add(item);
        }

        int reusedCount = materialized.Count + materialized.Count;
        int pagesAfterMaterializing = client.FetchedPageCount;

        await PrintResultAsync(
            $"duas enumeracoes: {firstPass} + {secondPass} itens, {pagesAfterTwoPasses} paginas buscadas");
        PrintLine(
            $"materializado e reutilizado: {reusedCount} itens lidos, {pagesAfterMaterializing} paginas buscadas");
    }

    private static async Task<int> CountAsync(IAsyncEnumerable<string> source)
    {
        int count = 0;
        await foreach (string item in source)
        {
            count++;
        }

        return count;
    }

    private SensorFeed CreateFeed()
    {
        return new SensorFeed(loggerFactory.CreateLogger<SensorFeed>(), ReadingInterval);
    }

    private PagedCatalogClient CreateCatalogClient()
    {
        return new PagedCatalogClient(
            loggerFactory.CreateLogger<PagedCatalogClient>(),
            PageLatency,
            PageSize,
            TotalPages);
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static async Task PrintResultAsync(string line)
    {
        await Task.Delay(LogFlushDelay);
        Console.WriteLine($">> {line}");
    }

    private static void PrintLine(string line)
    {
        Console.WriteLine($">> {line}");
    }
}
