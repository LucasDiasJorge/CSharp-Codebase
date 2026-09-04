namespace CancellationTokenPipeline.Models;

/// <summary>
/// Relatorio do que o pipeline conseguiu processar antes de terminar ou ser cancelado.
/// </summary>
public sealed class PipelineResult
{
    private PipelineResult(PipelineStatus status, int processedCount, int totalCount, string stageName)
    {
        Status = status;
        ProcessedCount = processedCount;
        TotalCount = totalCount;
        StageName = stageName;
    }

    public PipelineStatus Status { get; }

    public int ProcessedCount { get; }

    public int TotalCount { get; }

    /// <summary>Etapa em que o pipeline parou; vazia quando concluiu tudo.</summary>
    public string StageName { get; }

    public static PipelineResult Completed(int processedCount)
    {
        return new PipelineResult(PipelineStatus.Concluido, processedCount, processedCount, string.Empty);
    }

    public static PipelineResult Cancelled(PipelineStatus status, int processedCount, int totalCount, string stageName)
    {
        return new PipelineResult(status, processedCount, totalCount, stageName);
    }

    public string Describe()
    {
        if (Status == PipelineStatus.Concluido)
        {
            return $"{Status}: {ProcessedCount} documentos publicados";
        }

        return $"{Status}: {ProcessedCount}/{TotalCount} documentos concluidos, parada na etapa '{StageName}'";
    }
}
