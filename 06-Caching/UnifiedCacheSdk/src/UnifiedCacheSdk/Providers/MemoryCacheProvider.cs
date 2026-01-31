using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UnifiedCacheSdk.Abstractions;
using UnifiedCacheSdk.Options;

namespace UnifiedCacheSdk.Providers;

public sealed class MemoryCacheProvider(IMemoryCache cache, IOptions<UnifiedCacheOptions> options) : ICacheProvider
{
    private readonly IMemoryCache _cache = cache;
    private readonly UnifiedCacheOptions _options = options.Value;

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _options.DefaultTtl
        };
        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var exists = _cache.TryGetValue(key, out _);
        return Task.FromResult(exists);
    }
}
