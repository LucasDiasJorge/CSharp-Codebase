namespace IdempotencyCacheApi.Models;

public sealed class IdempotencyCacheEntry
{
    public required string RequestHash { get; init; }

    public required PaymentResponse Response { get; init; }

    public required DateTimeOffset CachedAtUtc { get; init; }
}
