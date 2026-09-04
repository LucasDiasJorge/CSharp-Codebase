using CancellationTokenPipeline.Models;
using Microsoft.Extensions.Logging;

namespace CancellationTokenPipeline.Pipeline;

/// <summary>
/// Etapa com trabalho item a item: como nao ha uma unica espera longa, o cancelamento e
/// verificado a cada iteracao com <see cref="CancellationToken.ThrowIfCancellationRequested"/>.
/// </summary>
public sealed class TransformDocumentsStage : IPipelineStage<IReadOnlyList<DocumentItem>, IReadOnlyList<DocumentItem>>
{
    private readonly ILogger<TransformDocumentsStage> logger;
    private readonly TimeSpan latencyPerItem;

    public TransformDocumentsStage(ILogger<TransformDocumentsStage> logger, TimeSpan latencyPerItem)
    {
        this.logger = logger;
        this.latencyPerItem = latencyPerItem;
    }

    public string Name => "Transform";

    public async Task<IReadOnlyList<DocumentItem>> ExecuteAsync(
        IReadOnlyList<DocumentItem> documents,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Stage}] transformando {Count} documentos", Name, documents.Count);

        foreach (DocumentItem document in documents)
        {
            // Ponto de cancelamento explicito: lanca OperationCanceledException carregando este token.
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(latencyPerItem, cancellationToken);
            document.ApplyTransformation(document.RawContent.ToUpperInvariant());
        }

        logger.LogInformation("[{Stage}] transformacao concluida", Name);
        return documents;
    }
}
