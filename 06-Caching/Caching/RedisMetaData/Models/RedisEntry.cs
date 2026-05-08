namespace RedisMetaData.Models;

public sealed class RedisEntry<TModel>
{
    public TModel? Data { get; init; }

    public DateTime? ExpiresAt { get; init; }
}