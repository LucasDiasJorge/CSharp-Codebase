using IdempotencyCacheApi.Models;

namespace IdempotencyCacheApi.Services;

public interface IIdempotencyService
{
    Task<IdempotencyExecutionResult> ExecuteAsync(
        string idempotencyKey,
        PaymentRequest request,
        Func<CancellationToken, Task<PaymentResponse>> handler,
        CancellationToken cancellationToken);

    bool Invalidate(string idempotencyKey);
}
