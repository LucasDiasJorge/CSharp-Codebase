using UnifiedCacheSdk.Abstractions;

namespace UnifiedCacheSdk;

public sealed class UnifiedCacheClient(ICacheProvider provider, ICacheKeyBuilder keyBuilder)
{
    private readonly ICacheProvider _provider = provider;
    private readonly ICacheKeyBuilder _keyBuilder = keyBuilder;

    public Task<T?> GetAsync<T>(string resource, string? scope = null, CancellationToken ct = default)
        => _provider.GetAsync<T>(_keyBuilder.Build(resource, scope), ct);

    public Task SetAsync<T>(string resource, T value, string? scope = null, TimeSpan? ttl = null, CancellationToken ct = default)
        => _provider.SetAsync(_keyBuilder.Build(resource, scope), value, ttl, ct);

    public Task RemoveAsync(string resource, string? scope = null, CancellationToken ct = default)
        => _provider.RemoveAsync(_keyBuilder.Build(resource, scope), ct);

    public Task<bool> ExistsAsync(string resource, string? scope = null, CancellationToken ct = default)
        => _provider.ExistsAsync(_keyBuilder.Build(resource, scope), ct);
}
