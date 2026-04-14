namespace IdempotencyCacheApi.Models;

public sealed class PaymentResponse
{
    public string TransactionId { get; init; } = string.Empty;

    public string OrderId { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTimeOffset ProcessedAtUtc { get; init; }

    public bool IsReplay { get; init; }
}
