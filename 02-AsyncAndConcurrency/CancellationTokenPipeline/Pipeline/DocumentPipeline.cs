using CancellationTokenPipeline.Models;
using Microsoft.Extensions.Logging;

namespace CancellationTokenPipeline.Pipeline;

/// <summary>
/// Orquestra as tres etapas repassando o mesmo token para todas. O pipeline nao engole
/// <see cref="OperationCanceledException"/>: quem iniciou o cancelamento e quem sabe classifica-lo,
/// entao a excecao sobe e o chamador decide o que reportar.
/// </summary>
public sealed class DocumentPipeline
{
    private readonly ILogger<DocumentPipeline> logger;
    private readonly IPipelineStage<int, IReadOnlyList<DocumentItem>> fetchStage;
    private readonly IPipelineStage<IReadOnlyList<DocumentItem>, IReadOnlyList<DocumentItem>> transformStage;
    private readonly IPipelineStage<IReadOnlyList<DocumentItem>, int> publishStage;

    public DocumentPipeline(
        ILogger<DocumentPipeline> logger,
        IPipelineStage<int, IReadOnlyList<DocumentItem>> fetchStage,
        IPipelineStage<IReadOnlyList<DocumentItem>, IReadOnlyList<DocumentItem>> transformStage,
        IPipelineStage<IReadOnlyList<DocumentItem>, int> publishStage)
    {
        this.logger = logger;
        this.fetchStage = fetchStage;
        this.transformStage = transformStage;
        this.publishStage = publishStage;
    }

    /// <summary>
    /// Guarda a etapa em execucao e os documentos ja carregados, para que o chamador
    /// consiga montar um relatorio parcial depois de um cancelamento.
    /// </summary>
    public string CurrentStageName { get; private set; } = string.Empty;

    public IReadOnlyList<DocumentItem> LoadedDocuments { get; private set; } = Array.Empty<DocumentItem>();

    public async Task<int> RunAsync(int documentCount, CancellationToken cancellationToken)
    {
        CurrentStageName = fetchStage.Name;
        LoadedDocuments = await fetchStage.ExecuteAsync(documentCount, cancellationToken);

        CurrentStageName = transformStage.Name;
        IReadOnlyList<DocumentItem> transformed = await transformStage.ExecuteAsync(LoadedDocuments, cancellationToken);

        CurrentStageName = publishStage.Name;
        int publishedCount = await publishStage.ExecuteAsync(transformed, cancellationToken);

        CurrentStageName = string.Empty;
        logger.LogInformation("Pipeline concluido com {Count} documentos", publishedCount);
        return publishedCount;
    }
}
