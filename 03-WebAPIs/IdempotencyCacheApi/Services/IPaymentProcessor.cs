using IdempotencyCacheApi.Models;

namespace IdempotencyCacheApi.Services;

public interface IPaymentProcessor
{
    Task<PaymentResponse> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken);
}
