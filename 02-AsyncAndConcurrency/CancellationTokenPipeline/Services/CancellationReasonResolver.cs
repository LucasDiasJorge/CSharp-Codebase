using CancellationTokenPipeline.Models;
using Microsoft.Extensions.Logging;

namespace CancellationTokenPipeline.Services;

/// <summary>
/// Descobre qual origem disparou o cancelamento.
///
/// Com um linked token, a <see cref="OperationCanceledException"/> carrega o token *ligado*,
/// e nao o token de origem. Por isso comparar <c>excecao.CancellationToken</c> com o token do
/// usuario ou do timeout nao funciona: e preciso perguntar a cada
/// <see cref="CancellationTokenSource"/> se ela foi cancelada.
/// </summary>
public sealed class CancellationReasonResolver
{
    private readonly ILogger<CancellationReasonResolver> logger;

    public CancellationReasonResolver(ILogger<CancellationReasonResolver> logger)
    {
        this.logger = logger;
    }

    public PipelineStatus Resolve(
        OperationCanceledException exception,
        CancellationTokenSource userSource,
        CancellationTokenSource timeoutSource)
    {
        logger.LogDebug(
            "Excecao {ExceptionType} capturada; token ligado cancelado: {LinkedCancelled}",
            exception.GetType().Name,
            exception.CancellationToken.IsCancellationRequested);

        if (userSource.IsCancellationRequested)
        {
            logger.LogWarning("Cancelamento originado pelo usuario");
            return PipelineStatus.CanceladoPeloUsuario;
        }

        if (timeoutSource.IsCancellationRequested)
        {
            logger.LogWarning("Cancelamento originado pelo timeout");
            return PipelineStatus.CanceladoPorTimeout;
        }

        // Nenhuma das origens conhecidas: o cancelamento veio de dentro de uma dependencia.
        logger.LogWarning("Origem do cancelamento nao identificada");
        return PipelineStatus.InterrompidoComResultadoParcial;
    }
}
