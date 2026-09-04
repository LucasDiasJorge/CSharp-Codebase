namespace CancellationTokenPipeline.Models;

/// <summary>
/// Resultado final observado pelo chamador do pipeline.
/// </summary>
public enum PipelineStatus
{
    /// <summary>Todas as etapas terminaram sem cancelamento.</summary>
    Concluido,

    /// <summary>O cancelamento partiu do token do usuario (Ctrl+C, botao de cancelar).</summary>
    CanceladoPeloUsuario,

    /// <summary>O cancelamento partiu do deadline configurado com CancelAfter.</summary>
    CanceladoPorTimeout,

    /// <summary>Encerramento cooperativo: a etapa parou sozinha e devolveu resultado parcial.</summary>
    InterrompidoComResultadoParcial
}
