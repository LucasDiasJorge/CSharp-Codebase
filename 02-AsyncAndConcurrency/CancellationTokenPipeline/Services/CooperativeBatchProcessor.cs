using CancellationTokenPipeline.Models;
using Microsoft.Extensions.Logging;

namespace CancellationTokenPipeline.Services;

/// <summary>
/// Alternativa ao <c>ThrowIfCancellationRequested</c>: em vez de lancar, o laco consulta
/// <see cref="CancellationToken.IsCancellationRequested"/>, para no proximo ponto seguro e
/// devolve o que ja concluiu. Use quando o resultado parcial tem valor para o chamador.
/// </summary>
public sealed class CooperativeBatchProcessor
{
    private readonly ILogger<CooperativeBatchProcessor> logger;
    private readonly TimeSpan latencyPerItem;

    public CooperativeBatchProcessor(ILogger<CooperativeBatchProcessor> logger, TimeSpan latencyPerItem)
    {
        this.logger = logger;
        this.latencyPerItem = latencyPerItem;
    }

    public async Task<PipelineResult> ProcessAsync(int documentCount, CancellationToken cancellationToken)
    {
        int processedCount = 0;

        for (int index = 1; index <= documentCount; index++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "Encerramento cooperativo apos {Processed} de {Total} documentos",
                    processedCount,
                    documentCount);

                return PipelineResult.Cancelled(
                    PipelineStatus.InterrompidoComResultadoParcial,
                    processedCount,
                    documentCount,
                    "CooperativeBatch");
            }

            // A espera nao recebe o token: o item corrente termina antes da parada,
            // evitando deixar trabalho pela metade.
            await Task.Delay(latencyPerItem, CancellationToken.None);
            processedCount++;
        }

        logger.LogInformation("Lote cooperativo concluido com {Processed} documentos", processedCount);
        return PipelineResult.Completed(processedCount);
    }
}
