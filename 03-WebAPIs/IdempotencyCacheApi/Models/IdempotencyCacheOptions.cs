namespace IdempotencyCacheApi.Models;

public sealed class IdempotencyCacheOptions
{
    public const string SectionName = "IdempotencyCache";

    public int AbsoluteExpirationMinutes { get; set; } = 10;

    public int SlidingExpirationMinutes { get; set; } = 2;
}
