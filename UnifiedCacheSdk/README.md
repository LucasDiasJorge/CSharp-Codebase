# UnifiedCacheSdk

SDK para homogenizar o uso de cache (Memory/Redis) com chaves padronizadas por serviço.

## Motivação
- Centralizar o acesso a cache e padronizar as chaves.
- Facilitar troca de provedor (Memory ou Redis) sem alterar o código do consumidor.
- Anexar automaticamente o `ServiceId` em todas as chaves.

## Instalação
Adicione o projeto `UnifiedCacheSdk` à sua solução e referencie-o no seu projeto.

## Uso (Minimal API / ASP.NET Core)
```csharp
builder.Services.AddUnifiedCache(options =>
{
    options.ServiceId = "pedido-api"; // identificador único do serviço
    options.GlobalPrefix = "cache";   // opcional, default: "cache"
    options.Separator = ":";          // opcional, default: ':'
    options.DefaultTtl = TimeSpan.FromMinutes(10);
}, cfg => cfg.UseRedis("localhost:6379")); // ou cfg.UseMemoryCache()

builder.Services.AddSingleton<UnifiedCacheClient>();
```

Consumo:
```csharp
public class PedidoService(UnifiedCacheClient cache)
{
    public async Task<Pedido?> ObterPedidoAsync(Guid id)
    {
        return await cache.GetAsync<Pedido>(resource: $"pedido:{id}");
        // chave resultante: "cache:pedido-api:pedido:{id}"
    }

    public async Task SalvarPedidoAsync(Pedido pedido)
    {
        await cache.SetAsync(resource: $"pedido:{pedido.Id}", value: pedido, ttl: TimeSpan.FromMinutes(5));
    }
}
```

Com escopo (ex: tenant, ambiente):
```csharp
await cache.SetAsync("usuario:123", user, scope: "tenant-1");
// resultado: "cache:pedido-api:tenant-1:usuario:123"
```

## Extensibilidade
- `ICacheProvider`: implemente para suportar outros provedores de cache.
- `ICacheKeyBuilder`: customize a forma de criar chaves.
- `UnifiedCacheOptions`: ajusta `ServiceId`, `GlobalPrefix`, `Separator`, `DefaultTtl`.

## Boas práticas
- Use `resource` descritivo e estável (ex: `usuario:{id}`, `pedido:{id}`).
- Defina `ServiceId` único por serviço para evitar colisões entre sistemas.
- Em Redis, prefira TTLs adequados ao dado para evitar memória desnecessária.
```