namespace SagaPattern.Core;

/// <summary>
/// Interface para um passo da Saga
/// Cada passo tem uma ação e uma compensação
/// </summary>
public interface ISagaStep<TContext>
{
    string Name { get; }
    Task<bool> ExecuteAsync(TContext context, CancellationToken ct = default);
    Task CompensateAsync(TContext context, CancellationToken ct = default);
}
