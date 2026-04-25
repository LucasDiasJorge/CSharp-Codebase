namespace UseCases.Core;

/// <summary>
/// Interface base para todos os Use Cases
/// </summary>
/// <typeparam name="TInput">Tipo do objeto de entrada (Request)</typeparam>
/// <typeparam name="TOutput">Tipo do objeto de saída (Response)</typeparam>
public interface IUseCase<TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface para Use Cases sem retorno
/// </summary>
/// <typeparam name="TInput">Tipo do objeto de entrada (Request)</typeparam>
public interface IUseCase<TInput>
{
    Task ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface para Use Cases sem entrada nem saída
/// </summary>
public interface IUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
