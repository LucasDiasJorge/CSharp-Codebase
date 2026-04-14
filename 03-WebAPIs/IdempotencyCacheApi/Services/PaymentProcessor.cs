using IdempotencyCacheApi.Models;

namespace IdempotencyCacheApi.Services;

public sealed class PaymentProcessor : IPaymentProcessor
{
    private readonly ILogger<PaymentProcessor> _logger;

    public PaymentProcessor(ILogger<PaymentProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentResponse> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(350), cancellationToken);

        string transactionId = $"txn_{Guid.NewGuid():N}";

        _logger.LogInformation(
            "Payment processed for order {OrderId} with transaction {TransactionId}.",
            request.OrderId,
            transactionId);

        return new PaymentResponse
        {
            TransactionId = transactionId,
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description ?? string.Empty,
            ProcessedAtUtc = DateTimeOffset.UtcNow,
            IsReplay = false
        };
    }
}
