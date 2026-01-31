namespace SagaPattern.Core;

/// <summary>
/// Interface do orquestrador de Saga
/// </summary>
public interface ISagaOrchestrator<TContext>
{
    Task<SagaResult> ExecuteAsync(TContext context, CancellationToken ct = default);
}
