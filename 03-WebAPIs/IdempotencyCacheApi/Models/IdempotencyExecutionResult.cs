namespace IdempotencyCacheApi.Models;

public sealed class IdempotencyExecutionResult
{
    private IdempotencyExecutionResult(
        IdempotencyExecutionStatus status,
        PaymentResponse? response,
        string? errorMessage)
    {
        Status = status;
        Response = response;
        ErrorMessage = errorMessage;
    }

    public IdempotencyExecutionStatus Status { get; }

    public PaymentResponse? Response { get; }

    public string? ErrorMessage { get; }

    public static IdempotencyExecutionResult Executed(PaymentResponse response)
    {
        return new IdempotencyExecutionResult(IdempotencyExecutionStatus.Executed, response, null);
    }

    public static IdempotencyExecutionResult Replay(PaymentResponse response)
    {
        return new IdempotencyExecutionResult(IdempotencyExecutionStatus.Replay, response, null);
    }

    public static IdempotencyExecutionResult Conflict(string errorMessage)
    {
        return new IdempotencyExecutionResult(IdempotencyExecutionStatus.Conflict, null, errorMessage);
    }
}
