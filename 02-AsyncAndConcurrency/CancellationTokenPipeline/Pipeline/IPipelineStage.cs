namespace CancellationTokenPipeline.Pipeline;

/// <summary>
/// Etapa assincrona do pipeline. O <see cref="CancellationToken"/> e o ultimo parametro
/// por convencao e deve ser repassado para toda chamada assincrona interna.
/// </summary>
public interface IPipelineStage<TInput, TOutput>
{
    string Name { get; }

    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
}
