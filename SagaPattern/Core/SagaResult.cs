namespace SagaPattern.Core;

/// <summary>
/// Resultado da execução de uma Saga
/// </summary>
public class SagaResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public SagaState FinalState { get; }
    public IReadOnlyList<string> ExecutedSteps { get; }
    public IReadOnlyList<string> CompensatedSteps { get; }

    private SagaResult(bool isSuccess, string? error, SagaState finalState, 
        IReadOnlyList<string> executedSteps, IReadOnlyList<string> compensatedSteps)
    {
        IsSuccess = isSuccess;
        Error = error;
        FinalState = finalState;
        ExecutedSteps = executedSteps;
        CompensatedSteps = compensatedSteps;
    }

    public static SagaResult Success(IReadOnlyList<string> executedSteps)
        => new(true, null, SagaState.Completed, executedSteps, []);

    public static SagaResult Failure(string error, IReadOnlyList<string> executedSteps, 
        IReadOnlyList<string> compensatedSteps)
        => new(false, error, SagaState.Compensated, executedSteps, compensatedSteps);

    public static SagaResult CompensationFailed(string error, IReadOnlyList<string> executedSteps, 
        IReadOnlyList<string> compensatedSteps)
        => new(false, error, SagaState.Failed, executedSteps, compensatedSteps);
}
