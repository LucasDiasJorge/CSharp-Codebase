using CancellationTokenPipeline.Models;
using Microsoft.Extensions.Logging;

namespace CancellationTokenPipeline.Pipeline;

/// <summary>
/// Etapa de I/O: repassa o token para <see cref="Task.Delay(TimeSpan, CancellationToken)"/>,
/// que lanca <see cref="TaskCanceledException"/> (subclasse de <see cref="OperationCanceledException"/>)
/// quando o cancelamento chega durante a espera.
/// </summary>
public sealed class FetchDocumentsStage : IPipelineStage<int, IReadOnlyList<DocumentItem>>
{
    private readonly ILogger<FetchDocumentsStage> logger;
    private readonly TimeSpan latencyPerBatch;

    public FetchDocumentsStage(ILogger<FetchDocumentsStage> logger, TimeSpan latencyPerBatch)
    {
        this.logger = logger;
        this.latencyPerBatch = latencyPerBatch;
    }

    public string Name => "Fetch";

    public async Task<IReadOnlyList<DocumentItem>> ExecuteAsync(int documentCount, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Stage}] buscando {Count} documentos na origem", Name, documentCount);

        // O token vai junto: sem ele a espera continuaria ate o fim mesmo apos o cancelamento.
        await Task.Delay(latencyPerBatch, cancellationToken);

        List<DocumentItem> documents = new List<DocumentItem>(documentCount);
        for (int index = 1; index <= documentCount; index++)
        {
            documents.Add(new DocumentItem(index, $"documento-bruto-{index:D2}"));
        }

        logger.LogInformation("[{Stage}] {Count} documentos carregados", Name, documents.Count);
        return documents;
    }
}
