<!-- README padronizado (versão condensada) -->
# FusionCache (Cache com Resiliência Avançada)

Projeto demonstrativo do [FusionCache](https://github.com/ZiggyCreatures/FusionCache), biblioteca de cache de alto nível para .NET que combina proteção contra cache stampede, fail-safe, background refresh e suporte multicamadas (memória + distribuído).

## 1. Visão Geral
FusionCache oferece API simplificada (`GetOrSet`/`GetOrSetAsync`) com recursos avançados integrados:
- **Anti-stampede**: apenas uma execução da factory por chave (previne thundering herd)
- **Fail-safe**: serve valores stale quando fonte primária falha
- **Background refresh**: renova itens proativamente antes da expiração
- **Multicamadas**: combina cache local (memória) com distribuído (Redis) + backplane para invalidação
- **Timeouts e jitter**: controle fino de resiliência sem depender de Polly

## 2. Objetivos Didáticos
- Demonstrar uso de `GetOrSet` com options builder
- Ilustrar `TryGet<T>` com `MaybeValue<T>`
- Mostrar fail-safe e timeout de factory
- Comparar com alternativas (`IMemoryCache`, Redis puro, LazyCache, EasyCaching)
- Evidenciar quando usar e quando evitar FusionCache

## 3. Estrutura Principal
```
FusionCache/
  Program.cs (exemplos de uso)
  README.md (comparativo e decisão)
```

## 4. Instalação
```powershell
dotnet add package ZiggyCreatures.FusionCache
# Para suporte distribuído (opcional):
dotnet add package ZiggyCreatures.FusionCache.Serialization.SystemTextJson
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

## 5. Exemplos de Uso

### GetOrSet Básico
```csharp
IFusionCache cache = services.GetRequiredService<IFusionCache>();
Product? product = await cache.GetOrSetAsync<Product>(
    "product:123",
    async (ctx, ct) => await _repository.GetByIdAsync(123, ct),
    options => options.SetDuration(TimeSpan.FromMinutes(10))
);
```

### TryGet com MaybeValue
```csharp
MaybeValue<Product> maybe = await cache.TryGetAsync<Product>("product:123");
if (maybe.HasValue)
{
    Product product = maybe.Value;
    // usar produto
}
```

### Fail-Safe e Timeout
```csharp
Product? product = await cache.GetOrSetAsync<Product>(
    "product:123",
    async (ctx, ct) => await _externalApi.GetProductAsync(123, ct),
    options => options
        .SetDuration(TimeSpan.FromMinutes(10))
        .SetFailSafe(true)                              // Serve stale em falha
        .SetFactoryTimeouts(TimeSpan.FromSeconds(5))    // Timeout da factory
);
```

## 6. Configuração no Program.cs
```csharp
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(options => options
        .SetDuration(TimeSpan.FromMinutes(5))
        .SetFailSafe(true)
    );

// Opcional: adicionar camada distribuída
builder.Services.AddStackExchangeRedisCache(options => 
{
    options.Configuration = "localhost:6379";
});
```

## 7. Quando Usar FusionCache

### ✅ Casos Ideais
- Alto tráfego sobre dados caros (APIs externas, queries complexas)
- Risco de cache stampede (muitos clientes solicitando mesma chave expirada)
- Necessidade de experiência resiliente durante falhas temporárias
- Aplicações distribuídas com invalidação coordenada entre instâncias
- Centralizar políticas de timeout e resiliência no próprio cache

### ❌ Quando Evitar
- Cache simples e local onde `IMemoryCache` atende 100%
- Necessidade de controle fino de estruturas Redis (Lua scripts, streams, Pub/Sub customizado)
- Overhead de dependência adicional não compensa benefícios
- Equipe não familiarizada e complexidade não justifica ganhos

## 8. Comparação com Alternativas

| Solução | Anti-Stampede | Fail-Safe | Background Refresh | Multicamadas | Complexidade |
|---------|---------------|-----------|--------------------|--------------|--------------| 
| **FusionCache** | ✅ | ✅ | ✅ | ✅ | Média |
| IMemoryCache | ❌ | ❌ | ❌ | ❌ | Baixa |
| Redis puro | ❌* | ❌ | ❌ | ✅ | Média |
| LazyCache | ✅ | ❌ | ❌ | ❌ | Baixa |
| EasyCaching | Parcial | ❌ | ❌ | ✅ | Média |

*Requer implementação manual com locks distribuídos

### IMemoryCache
- **Prós**: Built-in, leve, simples
- **Contras**: Sem recursos avançados; composição manual necessária

### Redis Puro / IDistributedCache
- **Prós**: Compartilhado entre instâncias, escalável
- **Contras**: Sem anti-stampede nativo; maior latência; precisa orquestrar políticas

### LazyCache
- **Prós**: API simples sobre `IMemoryCache` com lazy initialization
- **Contras**: Recursos de resiliência limitados; sem multicamadas

### EasyCaching
- **Prós**: Múltiplos backends (Redis, Memcached)
- **Contras**: Não oferece pacote integrado de anti-stampede + fail-safe + background refresh

## 9. Boas Práticas
- Defina TTLs realistas; evite durações excessivas sem necessidade
- Use factories assíncronas sempre que possível
- Ative `SetFailSafe(true)` onde experiência do usuário é crítica
- Configure `SetFactoryTimeouts` para evitar esperas indefinidas
- Evite payloads grandes; escolha serialização eficiente (System.Text.Json ou MessagePack)
- Em múltiplas instâncias, configure backplane para invalidação coordenada
- Monitore métricas (hit/miss, tempo de factory, eventos fail-safe)

## 10. Extensões Futuras
- Integrar telemetria OpenTelemetry
- Métricas customizadas (hit ratio, factory duration)
- Cache warming seletivo de chaves críticas
- Políticas de jitter para evitar picos de refresh simultâneo

## 11. Referências
- [FusionCache GitHub](https://github.com/ZiggyCreatures/FusionCache)
- [Documentação Oficial](https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/README.md)
- Cache Stampede (Wikipedia)
- Padrão Fail-Safe (resiliência)

## 12. Aprendizados Esperados
Após estudar: entender proteção anti-stampede, fail-safe pattern, background refresh, comparar trade-offs com outras soluções, decidir quando FusionCache agrega valor real.

---