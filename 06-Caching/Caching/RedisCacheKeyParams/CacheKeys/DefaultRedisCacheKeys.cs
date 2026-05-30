namespace RedisCacheKeyParams.CacheKeys;

public sealed class DefaultRedisCacheKeys : RedisCacheKeys
{
    public DefaultRedisCacheKeys(string separator = "_")
        : base(separator)
    {
    }

    public override string Namespace => "cache";
}