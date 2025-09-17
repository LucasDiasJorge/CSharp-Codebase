using Microsoft.Extensions.Options;
using UnifiedCacheSdk.Abstractions;
using UnifiedCacheSdk.Options;

namespace UnifiedCacheSdk.Core;

public sealed class CacheKeyBuilder(IOptions<UnifiedCacheOptions> options) : ICacheKeyBuilder
{
    private readonly UnifiedCacheOptions _options = options.Value;

    public string Build(string resource, string? scope = null)
    {
        var parts = new List<string>(4)
        {
            _options.GlobalPrefix,
            _options.ServiceId
        };

        if (!string.IsNullOrWhiteSpace(scope))
            parts.Add(scope);

        parts.Add(resource);

        return string.Join(_options.Separator, parts);
    }
}
