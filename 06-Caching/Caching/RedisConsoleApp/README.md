# RedisConsoleApp

## Visão geral

Aplicação console que demonstra operações essenciais e padrões práticos com Redis usando `StackExchange.Redis`.

## Conceitos abordados

- Exemplo didático sobre RedisConsoleApp no contexto de estratégias de cache e integração com Redis.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Conectar, manipular tipos primitivos e estruturas (String, Hash, List, Set, SortedSet).
- Implementar cache-aside básico e rate limiting.
- Mostrar Pub/Sub e sessões simples.

## Estrutura do projeto

```text
RedisConsoleApp/
+-- Program.cs
\-- RedisConsoleApp.csproj
```

## Como executar

```bash
dotnet run --project 06-Caching/Caching/RedisConsoleApp/RedisConsoleApp.csproj
```

## Boas práticas e pontos de atenção

| Tema | Nota |
|------|------|
| ConnectionMultiplexer | Singleton reutilizável |
| Serialização | System.Text.Json com DTOs claros |
| TTL | Ajustar por domínio (sessões < cache catálogo) |
| Observabilidade | Log de erros + métricas de latência |
| Fallback | Em erro, usar fonte primária e continuar |

## Conteúdo complementar

##### 2. Dependência

```powershell
dotnet add package StackExchange.Redis
```

##### 3. Subir Redis (Docker)

```powershell
docker run -d --name redis -p 6379:6379 redis
```

##### 4. Operações Básicas

```csharp
ConnectionMultiplexer mux = ConnectionMultiplexer.Connect("localhost:6379");
IDatabase db = mux.GetDatabase();
await db.StringSetAsync("key", "value", TimeSpan.FromMinutes(30));
RedisValue v = await db.StringGetAsync("key");
```

##### 5. Cache Service (Esboço)

```csharp
public async Task<T?> GetAsync<T>(string key)
{
    RedisValue raw = await _db.StringGetAsync(key);
    return raw.HasValue ? JsonSerializer.Deserialize<T>(raw!) : default(T);
}
```

##### 6. Rate Limiting (Sliding Window simples)

Armazena timestamps em SortedSet e expira janela:
```csharp
long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
await _db.SortedSetRemoveRangeByScoreAsync(key, 0, now - windowSecs);
```

##### 7. Pub/Sub

```csharp
await sub.PublishAsync("channel", JsonSerializer.Serialize(payload));
await sub.SubscribeAsync("channel", (ch, val) => { /* handler */ });
```

##### 9. Próximos Passos

- Adicionar circuit breaker (Polly) para falhas sucessivas.
- Implementar `INCREMENT` para contadores de uso.
- Migrar Rate Limit para token bucket.
- Expor métricas (Prometheus) de hits/misses.

## Referências

StackExchange.Redis docs, Redis.io, exemplos de caching distribuído em .NET.
