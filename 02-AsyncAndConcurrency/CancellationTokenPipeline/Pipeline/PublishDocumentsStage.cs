using CancellationTokenPipeline.Models;
using Microsoft.Extensions.Logging;

namespace CancellationTokenPipeline.Pipeline;

/// <summary>
/// Etapa que mostra dois detalhes praticos do cancelamento: um callback registrado com
/// <see cref="CancellationToken.Register(Action)"/> e uma limpeza obrigatoria que roda no
/// <c>finally</c> com <see cref="CancellationToken.None"/>, para nao ser cancelada tambem.
/// </summary>
public sealed class PublishDocumentsStage : IPipelineStage<IReadOnlyList<DocumentItem>, int>
{
    private readonly ILogger<PublishDocumentsStage> logger;
    private readonly TimeSpan latencyPerItem;

    public PublishDocumentsStage(ILogger<PublishDocumentsStage> logger, TimeSpan latencyPerItem)
    {
        this.logger = logger;
        this.latencyPerItem = latencyPerItem;
    }

    public string Name => "Publish";

    public async Task<int> ExecuteAsync(IReadOnlyList<DocumentItem> documents, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Stage}] publicando {Count} documentos", Name, documents.Count);

        // O callback dispara assim que o token e cancelado, sem esperar a proxima verificacao.
        // A registration precisa ser liberada para nao vazar enquanto o CTS viver.
        using CancellationTokenRegistration registration = cancellationToken.Register(
            () => logger.LogWarning("[{Stage}] callback de cancelamento disparado", Name));

        int publishedCount = 0;

        try
        {
            foreach (DocumentItem document in documents)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(latencyPerItem, cancellationToken);
                document.MarkAsPublished();
                publishedCount++;
            }

            logger.LogInformation("[{Stage}] {Count} documentos publicados", Name, publishedCount);
            return publishedCount;
        }
        finally
        {
            // Compensacao/flush deve completar mesmo apos o cancelamento: por isso CancellationToken.None.
            await FlushPublicationLogAsync(publishedCount, CancellationToken.None);
        }
    }

    private async Task FlushPublicationLogAsync(int publishedCount, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
        logger.LogInformation("[{Stage}] log de publicacao gravado para {Count} documentos", Name, publishedCount);
    }
}
