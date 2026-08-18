# CLAUDE.md — UnifiedCacheSdk

Biblioteca que unifica cache em memória e Redis atrás de uma única abstração. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 06-Caching/UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj
```

**Biblioteca, não executável** — não há `dotnet run`, e o sample não inclui projeto de demo ou de testes.

## Estrutura interna

- `Abstractions/ICacheProvider.cs` — o contrato que os dois backends implementam.
- `Providers/MemoryCacheProvider.cs` e `Providers/RedisCacheProvider.cs` — as implementações. Trocar de backend é trocar o registro, sem tocar no código consumidor.
- `Abstractions/ICacheKeyBuilder.cs` + `Core/CacheKeyBuilder.cs` — composição consistente de chaves (compare com `RedisCacheKeyParams`).
- `Options/UnifiedCacheOptions.cs` — configuração tipada.
- `Extensions/ServiceCollectionExtensions.cs` — o `AddUnifiedCache(...)` que amarra tudo no contêiner de DI. **É a porta de entrada da biblioteca**; comece a leitura por aqui.
- `UnifiedCacheClient.cs` — fachada usada pelo consumidor.

## Pontos de atenção

- TFM `net9.0`. Depende só de abstrações `Microsoft.Extensions.*` (9.0.1) e `StackExchange.Redis` 2.7.33 — desenho correto para biblioteca.
- **Sem README local**; a documentação está em `06-Caching/UnifiedCacheSdk/`.
- Layout `src/`: aponte comandos para o `.csproj` interno.
- É biblioteca reutilizável dentro da trilha de caching — compare o desenho com `13-SDKsAndLibraries/MySimpleSdk`, que segue a mesma intenção.
