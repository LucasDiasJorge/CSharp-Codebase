# 06-Caching

## Visão geral

Trilha dedicada a estratégias de cache em .NET (ex.: .NET 10). O repositório contém exemplos práticos que cobrem cache em memória, cache distribuído com Redis, integração com banco relacional, resiliência contra falhas e uma pequena SDK para unificar provedores.

Esta pasta está organizada em dois blocos principais: a coleção [Caching](./Caching/README.md), que reúne os exemplos executáveis de estudo, e o [UnifiedCacheSdk](./UnifiedCacheSdk/README.md), que demonstra uma abordagem reutilizável para uniformizar o acesso a provedores de cache.

## Conceitos abordados

- `IMemoryCache`, Redis e cache distribuído.
- Cache-Aside, contadores atômicos, comparativo de padrões e cache híbrido/resiliente.
- TTL, invalidação seletiva, hit/miss ratio e fallback em caso de falha do cache.
- Padronização de chaves, isolamento por serviço e abstração de provedor.

## Objetivos de aprendizagem

- Entender quando usar cada estratégia de cache de acordo com o cenário.
- Executar exemplos isolados com comandos direcionados ao `.csproj` correto.
- Comparar soluções simples, distribuídas e resilientes dentro da mesma trilha.
- Reaproveitar a documentação local como mapa de estudo e referência prática.

## Estrutura do repositório

```text
06-Caching/
+-- Caching/
|   +-- README.md
|   +-- CacheAside/
|   +-- CacheIncrement/
|   +-- CachePatterns/
|   +-- FusionCache/
|   +-- RedisConsoleApp/
|   \-- RedisMySQLIntegration/
+-- UnifiedCacheSdk/
|   +-- README.md
|   \-- src/
\-- README.md
```

## Como executar

Executar a partir da raiz do repositório (`06-Caching`). Exemplos principais:

```bash
dotnet run --project Caching/CacheAside/CacheAside.csproj
dotnet run --project Caching/CacheIncrement/CacheIncrement.csproj
dotnet run --project Caching/CachePatterns/CachePatterns.csproj
dotnet run --project Caching/FusionCache/FusionCache.csproj
dotnet run --project Caching/RedisConsoleApp/RedisConsoleApp.csproj
dotnet run --project Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj
```

SDK reutilizável:

```bash
dotnet build UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj
```

> Observação: alguns exemplos exigem serviços externos (Redis, MySQL). Use Docker para facilitar a execução local.

## Boas práticas e pontos de atenção

- Use chaves previsíveis e estruturadas, como `produto:123` ou `pedido:tenant-a:987`.
- Defina TTL de acordo com a volatilidade do dado, evitando expiração longa para dados muito dinâmicos.
- Prefira invalidação seletiva em vez de limpar o cache inteiro sem necessidade.
- Mantenha fallback para banco ou fonte original quando o cache falhar ou estiver indisponível.
- Monitore latência, hit/miss ratio e custo de serialização antes de ampliar o uso de cache.
- Valide dependências externas antes de executar (Redis, MySQL quando aplicável).

## Conteúdo complementar

### Mapa rápido da trilha

| Projeto | Foco principal | Dependências | Comando principal |
|---------|----------------|--------------|-------------------|
| [Caching](./Caching/README.md) | Visão geral da coleção de exemplos | Varia por subprojeto | Consulte o README da pasta |
| [CacheAside](./Caching/CacheAside/README.md) | Padrão Cache-Aside em API | Memória local | `dotnet run --project Caching/CacheAside/CacheAside.csproj` |
| [CacheIncrement](./Caching/CacheIncrement/README.md) | Contadores de alta performance | Redis + MySQL | `dotnet run --project Caching/CacheIncrement/CacheIncrement.csproj` |
| [CachePatterns](./Caching/CachePatterns/README.md) | Comparativo de estratégias | Console local | `dotnet run --project Caching/CachePatterns/CachePatterns.csproj` |
| [FusionCache](./Caching/FusionCache/README.md) | Resiliência e anti-stampede | Biblioteca FusionCache | `dotnet run --project Caching/FusionCache/FusionCache.csproj` |
| [RedisConsoleApp](./Caching/RedisConsoleApp/README.md) | Operações essenciais com Redis | Redis | `dotnet run --project Caching/RedisConsoleApp/RedisConsoleApp.csproj` |
| [RedisMySQLIntegration](./Caching/RedisMySQLIntegration/README.md) | Cache distribuído com persistência | Redis + MySQL | `dotnet run --project Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj` |
| [UnifiedCacheSdk](./UnifiedCacheSdk/README.md) | SDK para unificar acesso a cache | Memory ou Redis | `dotnet build UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj` |

### Ordem de estudo recomendada

1. [CacheAside](./Caching/CacheAside/README.md) → Fundamentos e padrão mais comum
2. [CachePatterns](./Caching/CachePatterns/README.md) → Visão comparativa de estratégias
3. [RedisConsoleApp](./Caching/RedisConsoleApp/README.md) → Recursos do Redis
4. [RedisMySQLIntegration](./Caching/RedisMySQLIntegration/README.md) → Cache distribuído com persistência
5. [CacheIncrement](./Caching/CacheIncrement/README.md) → Escrita intensiva e sincronização
6. [FusionCache](./Caching/FusionCache/README.md) → Resiliência e recursos avançados
7. [UnifiedCacheSdk](./UnifiedCacheSdk/README.md) → Padronização e reuso em soluções maiores

## Referências e documentação complementar

- [Microsoft Docs - Overview of caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/overview)
- [Redis documentation](https://redis.io/docs/)
- [FusionCache GitHub](https://github.com/ZiggyCreatures/FusionCache)
- [Caching/README.md](./Caching/README.md)
- [UnifiedCacheSdk/README.md](./UnifiedCacheSdk/README.md)