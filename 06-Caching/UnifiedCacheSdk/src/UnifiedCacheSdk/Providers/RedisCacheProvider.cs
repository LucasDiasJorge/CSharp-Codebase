using System.Text.Json;
using StackExchange.Redis;
using UnifiedCacheSdk.Abstractions;

namespace UnifiedCacheSdk.Providers;

public sealed class RedisCacheProvider(IConnectionMultiplexer connection) : ICacheProvider
{
    private readonly IDatabase _db = connection.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key).ConfigureAwait(false);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, ttl).ConfigureAwait(false);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => _db.KeyDeleteAsync(key);

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        => await _db.KeyExistsAsync(key).ConfigureAwait(false);
}
