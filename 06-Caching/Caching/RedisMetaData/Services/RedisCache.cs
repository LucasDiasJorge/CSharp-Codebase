using System.Text.Json;
using RedisMetaData.Models;
using StackExchange.Redis;

namespace RedisMetaData.Services;

public sealed class RedisCache : IDisposable
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _database;

    public RedisCache(RedisConfig config)
    {
        _connection = ConnectionMultiplexer.Connect(config.ConnectionString);
        _database = _connection.GetDatabase(config.Database);
    }

    public bool IsConnected => _connection.IsConnected;

    public void SetEntry<TModel>(string redisKey, string hashField, TModel model, TimeSpan? ttl = null)
    {
        RedisEntry<TModel> entry = new()
        {
            Data = model,
            ExpiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null
        };

        _database.HashSet(BuildKey(redisKey), hashField, JsonSerializer.Serialize(entry));
    }

    public TModel? GetEntry<TModel>(string redisKey, string hashField)
    {
        RedisValue raw = _database.HashGet(BuildKey(redisKey), hashField);

        if (raw.IsNullOrEmpty)
        {
            return default;
        }

        RedisEntry<TModel>? entry = JsonSerializer.Deserialize<RedisEntry<TModel>>(raw.ToString());

        if (entry is null)
        {
            return default;
        }

        if (entry.ExpiresAt.HasValue && DateTime.UtcNow > entry.ExpiresAt.Value)
        {
            _database.HashDelete(BuildKey(redisKey), hashField);
            return default;
        }

        return entry.Data;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private static string BuildKey(string redisKey) => $"main-key:{redisKey}";
}