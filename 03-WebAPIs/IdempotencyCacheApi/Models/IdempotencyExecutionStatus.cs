namespace IdempotencyCacheApi.Models;

public enum IdempotencyExecutionStatus
{
    Executed = 1,
    Replay = 2,
    Conflict = 3
}
