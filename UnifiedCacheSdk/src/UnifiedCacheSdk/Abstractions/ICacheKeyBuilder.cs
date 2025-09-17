namespace UnifiedCacheSdk.Abstractions;

public interface ICacheKeyBuilder
{
    string Build(string resource, string? scope = null);
}
