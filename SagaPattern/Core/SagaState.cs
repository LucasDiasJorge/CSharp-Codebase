namespace SagaPattern.Core;

/// <summary>
/// Estados possíveis de uma Saga
/// </summary>
public enum SagaState
{
    NotStarted,
    Running,
    Completed,
    Compensating,
    Compensated,
    Failed
}
