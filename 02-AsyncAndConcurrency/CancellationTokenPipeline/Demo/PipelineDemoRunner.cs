using CancellationTokenPipeline.Models;
using CancellationTokenPipeline.Pipeline;
using CancellationTokenPipeline.Services;
using Microsoft.Extensions.Logging;

namespace CancellationTokenPipeline.Demo;

/// <summary>
/// Executa os cenarios de cancelamento em sequencia, do fluxo feliz ate o encerramento
/// cooperativo com resultado parcial.
/// </summary>
public sealed class PipelineDemoRunner
{
    private const int DocumentCount = 6;

    private static readonly TimeSpan FetchLatency = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan ItemLatency = TimeSpan.FromMilliseconds(120);

    /// <summary>
    /// O provider de console do <c>Microsoft.Extensions.Logging</c> grava em uma fila processada
    /// por thread propria. Sem esta pausa, o resumo escrito direto no <see cref="Console"/>
    /// apareceria antes dos logs da etapa que ele resume.
    /// </summary>
    private static readonly TimeSpan LogFlushDelay = TimeSpan.FromMilliseconds(100);

    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<PipelineDemoRunner> logger;
    private readonly CancellationReasonResolver reasonResolver;

    public PipelineDemoRunner(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        logger = loggerFactory.CreateLogger<PipelineDemoRunner>();
        reasonResolver = new CancellationReasonResolver(loggerFactory.CreateLogger<CancellationReasonResolver>());
    }

    public async Task RunAllAsync()
    {
        await RunHappyPathAsync();
        await RunTimeoutCancellationAsync();
        await RunUserCancellationAsync();
        await RunCooperativeShutdownAsync();
        await RunAlreadyCancelledTokenAsync();
    }

    /// <summary>Cenario 1: nenhum cancelamento, o token apenas atravessa as etapas.</summary>
    private async Task RunHappyPathAsync()
    {
        PrintHeader("1. Fluxo completo, sem cancelamento");

        DocumentPipeline pipeline = CreatePipeline();
        using CancellationTokenSource userSource = new CancellationTokenSource();

        int publishedCount = await pipeline.RunAsync(DocumentCount, userSource.Token);
        await PrintResultAsync(PipelineResult.Completed(publishedCount));
    }

    /// <summary>
    /// Cenario 2: linked token entre usuario e deadline. Quem cancela e o timeout,
    /// e o resolver identifica isso consultando as sources, nao a excecao.
    /// </summary>
    private async Task RunTimeoutCancellationAsync()
    {
        PrintHeader("2. Cancelamento por timeout (linked token)");

        DocumentPipeline pipeline = CreatePipeline();
        using CancellationTokenSource userSource = new CancellationTokenSource();
        using CancellationTokenSource timeoutSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(700));
        using CancellationTokenSource linkedSource =
            CancellationTokenSource.CreateLinkedTokenSource(userSource.Token, timeoutSource.Token);

        await RunAndReportAsync(pipeline, linkedSource.Token, userSource, timeoutSource);
    }

    /// <summary>
    /// Cenario 3: mesma montagem do cenario 2, mas agora quem cancela e o usuario,
    /// antes de o deadline expirar.
    /// </summary>
    private async Task RunUserCancellationAsync()
    {
        PrintHeader("3. Cancelamento pelo usuario (linked token)");

        DocumentPipeline pipeline = CreatePipeline();
        using CancellationTokenSource userSource = new CancellationTokenSource();
        using CancellationTokenSource timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using CancellationTokenSource linkedSource =
            CancellationTokenSource.CreateLinkedTokenSource(userSource.Token, timeoutSource.Token);

        // Simula o Ctrl+C do operador no meio da execucao.
        userSource.CancelAfter(TimeSpan.FromMilliseconds(1300));

        await RunAndReportAsync(pipeline, linkedSource.Token, userSource, timeoutSource);
    }

    /// <summary>Cenario 4: parada cooperativa que devolve resultado parcial em vez de lancar.</summary>
    private async Task RunCooperativeShutdownAsync()
    {
        PrintHeader("4. Encerramento cooperativo com resultado parcial");

        CooperativeBatchProcessor processor = new CooperativeBatchProcessor(
            loggerFactory.CreateLogger<CooperativeBatchProcessor>(),
            ItemLatency);

        using CancellationTokenSource shutdownSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));

        PipelineResult result = await processor.ProcessAsync(DocumentCount, shutdownSource.Token);
        await PrintResultAsync(result);
    }

    /// <summary>
    /// Cenario 5: token ja cancelado antes da primeira etapa. O pipeline nem chega a fazer
    /// trabalho util, e o filtro de excecao separa cancelamento esperado de falha real.
    /// </summary>
    private async Task RunAlreadyCancelledTokenAsync()
    {
        PrintHeader("5. Token ja cancelado antes de iniciar");

        DocumentPipeline pipeline = CreatePipeline();
        using CancellationTokenSource cancelledSource = new CancellationTokenSource();
        await cancelledSource.CancelAsync();

        try
        {
            await pipeline.RunAsync(DocumentCount, cancelledSource.Token);
        }
        catch (OperationCanceledException) when (cancelledSource.IsCancellationRequested)
        {
            // Filtro de excecao: so tratamos como cancelamento esperado quando a origem foi a nossa.
            logger.LogWarning("Nenhum documento foi buscado; o token ja estava cancelado");
            await PrintResultAsync(PipelineResult.Cancelled(
                PipelineStatus.CanceladoPeloUsuario,
                processedCount: 0,
                totalCount: DocumentCount,
                pipeline.CurrentStageName));
        }
    }

    private async Task RunAndReportAsync(
        DocumentPipeline pipeline,
        CancellationToken cancellationToken,
        CancellationTokenSource userSource,
        CancellationTokenSource timeoutSource)
    {
        try
        {
            int publishedCount = await pipeline.RunAsync(DocumentCount, cancellationToken);
            await PrintResultAsync(PipelineResult.Completed(publishedCount));
        }
        catch (OperationCanceledException exception)
        {
            PipelineStatus status = reasonResolver.Resolve(exception, userSource, timeoutSource);
            int publishedCount = CountPublished(pipeline.LoadedDocuments);

            await PrintResultAsync(PipelineResult.Cancelled(
                status,
                publishedCount,
                DocumentCount,
                pipeline.CurrentStageName));
        }
    }

    private DocumentPipeline CreatePipeline()
    {
        return new DocumentPipeline(
            loggerFactory.CreateLogger<DocumentPipeline>(),
            new FetchDocumentsStage(loggerFactory.CreateLogger<FetchDocumentsStage>(), FetchLatency),
            new TransformDocumentsStage(loggerFactory.CreateLogger<TransformDocumentsStage>(), ItemLatency),
            new PublishDocumentsStage(loggerFactory.CreateLogger<PublishDocumentsStage>(), ItemLatency));
    }

    private static int CountPublished(IReadOnlyList<DocumentItem> documents)
    {
        int publishedCount = 0;
        foreach (DocumentItem document in documents)
        {
            if (document.IsPublished)
            {
                publishedCount++;
            }
        }

        return publishedCount;
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static async Task PrintResultAsync(PipelineResult result)
    {
        await Task.Delay(LogFlushDelay);
        Console.WriteLine($">> {result.Describe()}");
    }
}
