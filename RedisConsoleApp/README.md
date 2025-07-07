# Redis Console Application

## 📚 Conceitos Abordados

Este projeto demonstra o uso do Redis (Remote Dictionary Server) com .NET:

- **Redis**: Banco de dados em memória key-value
- **StackExchange.Redis**: Cliente oficial para .NET
- **Caching**: Cache distribuído e local
- **Data Structures**: Strings, Lists, Sets, Hashes, Sorted Sets
- **Pub/Sub**: Sistema de mensageria
- **Expiration**: TTL (Time To Live) para chaves
- **Persistence**: Snapshots e AOF

## 🎯 Objetivos de Aprendizado

- Configurar e conectar ao Redis
- Implementar estratégias de caching
- Trabalhar com diferentes estruturas de dados
- Usar Redis para sessões distribuídas
- Implementar pub/sub messaging
- Otimizar performance de aplicações

## 💡 Conceitos Importantes

### Connection Setup
```csharp
var redis = ConnectionMultiplexer.Connect("localhost:6379");
var db = redis.GetDatabase();
```

### Basic Operations
```csharp
// Set/Get
await db.StringSetAsync("key", "value");
var value = await db.StringGetAsync("key");

// Set with expiration
await db.StringSetAsync("session:123", "user_data", TimeSpan.FromMinutes(30));

// Increment
await db.StringIncrementAsync("counter", 1);
```

### Complex Data Types
```csharp
// Hash (Dictionary-like)
await db.HashSetAsync("user:1", new HashEntry[]
{
    new("name", "John"),
    new("email", "john@example.com"),
    new("age", "30")
});

// List (Queue-like)
await db.ListLeftPushAsync("queue", "item1");
var item = await db.ListRightPopAsync("queue");

// Set (Unique collection)
await db.SetAddAsync("tags", "redis");
var isMember = await db.SetContainsAsync("tags", "redis");
```

## 🚀 Como Executar

### 1. Instalar Redis
```bash
# Docker
docker run --name redis -p 6379:6379 -d redis

# Windows: https://redis.io/docs/latest/operate/oss_and_stack/install/archive/install-redis/install-redis-on-windows/
```

### 2. Instalar Package
```bash
dotnet add package StackExchange.Redis
```

### 3. Executar
```bash
cd RedisConsoleApp
dotnet run
```

## 📖 Implementações Práticas

### 1. Cache Service
```csharp
public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;

    public RedisCacheService(ConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (!value.HasValue)
            return default(T);

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, serializedValue, expiration);
    }
}
```

### 2. Session Manager
```csharp
public class RedisSessionManager
{
    private readonly IDatabase _database;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public async Task<string> CreateSessionAsync(string userId, Dictionary<string, object> data)
    {
        var sessionId = Guid.NewGuid().ToString();
        var sessionKey = $"session:{sessionId}";

        var sessionData = new
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Data = data
        };

        await _database.StringSetAsync(sessionKey, 
            JsonSerializer.Serialize(sessionData), 
            _defaultExpiration);

        return sessionId;
    }
}
```

### 3. Pub/Sub Messaging
```csharp
public class RedisPubSubService
{
    private readonly ISubscriber _subscriber;

    public RedisPubSubService(ConnectionMultiplexer redis)
    {
        _subscriber = redis.GetSubscriber();
    }

    public async Task PublishAsync<T>(string channel, T message)
    {
        var serializedMessage = JsonSerializer.Serialize(message);
        await _subscriber.PublishAsync(channel, serializedMessage);
    }

    public async Task SubscribeAsync<T>(string channel, Action<T> onMessage)
    {
        await _subscriber.SubscribeAsync(channel, (redisChannel, value) =>
        {
            var message = JsonSerializer.Deserialize<T>(value);
            onMessage(message);
        });
    }
}
```

## 🏗️ Padrões de Uso

### Cache-Aside Pattern
```csharp
public async Task<Product> GetProductAsync(int id)
{
    var cacheKey = $"product:{id}";
    
    // Try cache first
    var cachedProduct = await _cache.GetAsync<Product>(cacheKey);
    if (cachedProduct != null)
        return cachedProduct;

    // Load from database
    var product = await _repository.GetByIdAsync(id);
    if (product != null)
    {
        // Store in cache
        await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(15));
    }

    return product;
}
```

### Rate Limiting
```csharp
public async Task<bool> IsAllowedAsync(string identifier, int maxRequests, TimeSpan window)
{
    var key = $"rate_limit:{identifier}";
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var windowStart = now - (long)window.TotalSeconds;

    // Remove old entries
    await _database.SortedSetRemoveRangeByScoreAsync(key, 0, windowStart);

    // Count current requests
    var currentCount = await _database.SortedSetLengthAsync(key);

    if (currentCount >= maxRequests)
        return false;

    // Add current request
    await _database.SortedSetAddAsync(key, Guid.NewGuid().ToString(), now);
    await _database.KeyExpireAsync(key, window);

    return true;
}
```

## 🔍 Pontos de Atenção

### Connection Management
```csharp
// ✅ Use singleton ConnectionMultiplexer
builder.Services.AddSingleton<ConnectionMultiplexer>(provider =>
{
    var connectionString = provider.GetRequiredService<IConfiguration>()
        .GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString);
});
```

### Error Handling
```csharp
public async Task<T> SafeGetAsync<T>(string key, Func<Task<T>> fallback)
{
    try
    {
        return await _cache.GetAsync<T>(key);
    }
    catch (RedisException ex)
    {
        _logger.LogWarning(ex, "Redis operation failed, using fallback");
        return await fallback();
    }
}
```

## 📚 Recursos Adicionais

- [StackExchange.Redis Documentation](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis Documentation](https://redis.io/documentation)
- [Redis Installation Guide](https://redis.io/docs/latest/operate/oss_and_stack/install/archive/install-redis/install-redis-on-windows/)