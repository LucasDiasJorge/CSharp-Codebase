# 06-Caching

## Visão geral

Trilha do repositório dedicada a estratégias de cache em .NET, com exemplos que cobrem cache em memória, cache distribuído com Redis, integração com banco relacional, resiliência contra falhas e abstrações reutilizáveis para padronização de chaves.

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

## Estrutura do projeto

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

Coleção de exemplos executáveis:

```bash
dotnet run --project 06-Caching/Caching/CacheAside/CacheAside.csproj
dotnet run --project 06-Caching/Caching/CacheIncrement/CacheIncrement.csproj
dotnet run --project 06-Caching/Caching/CachePatterns/CachePatterns.csproj
dotnet run --project 06-Caching/Caching/FusionCache/FusionCache.csproj
dotnet run --project 06-Caching/Caching/RedisConsoleApp/RedisConsoleApp.csproj
dotnet run --project 06-Caching/Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj
```

SDK reutilizável:

```bash
dotnet build 06-Caching/UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj
```

## Boas práticas e pontos de atenção

- Use chaves previsíveis e estruturadas, como `produto:123` ou `pedido:tenant-a:987`.
- Defina TTL de acordo com a volatilidade do dado, evitando expiração longa para dados muito dinâmicos.
- Prefira invalidação seletiva em vez de limpar o cache inteiro sem necessidade.
- Mantenha fallback para banco ou fonte original quando o cache falhar ou estiver indisponível.
- Monitore latência, hit/miss ratio e custo de serialização antes de ampliar o uso de cache.
- Alguns exemplos exigem Redis e MySQL ativos; valide dependências externas antes de executar.

## Conteúdo complementar

### Mapa rápido da trilha

| Projeto | Foco principal | Dependências | Comando principal |
|---------|----------------|--------------|-------------------|
| [Caching](./Caching/README.md) | Visão geral da coleção de exemplos | Varia por subprojeto | Consulte o README da pasta |
| [CacheAside](./Caching/CacheAside/README.md) | Padrão Cache-Aside em API | Memória local | `dotnet run --project 06-Caching/Caching/CacheAside/CacheAside.csproj` |
| [CacheIncrement](./Caching/CacheIncrement/README.md) | Contadores de alta performance | Redis + MySQL | `dotnet run --project 06-Caching/Caching/CacheIncrement/CacheIncrement.csproj` |
| [CachePatterns](./Caching/CachePatterns/README.md) | Comparativo de estratégias | Console local | `dotnet run --project 06-Caching/Caching/CachePatterns/CachePatterns.csproj` |
| [FusionCache](./Caching/FusionCache/README.md) | Resiliência e anti-stampede | Biblioteca FusionCache | `dotnet run --project 06-Caching/Caching/FusionCache/FusionCache.csproj` |
| [RedisConsoleApp](./Caching/RedisConsoleApp/README.md) | Operações essenciais com Redis | Redis | `dotnet run --project 06-Caching/Caching/RedisConsoleApp/RedisConsoleApp.csproj` |
| [RedisMySQLIntegration](./Caching/RedisMySQLIntegration/README.md) | Cache distribuído com persistência | Redis + MySQL | `dotnet run --project 06-Caching/Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj` |
| [UnifiedCacheSdk](./UnifiedCacheSdk/README.md) | SDK para unificar acesso a cache | Memory ou Redis | `dotnet build 06-Caching/UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj` |

### Ordem de estudo recomendada

1. [CacheAside](./Caching/CacheAside/README.md) para entender o padrão mais comum.
2. [CachePatterns](./Caching/CachePatterns/README.md) para comparar abordagens.
3. [RedisConsoleApp](./Caching/RedisConsoleApp/README.md) para revisar operações básicas no Redis.
4. [RedisMySQLIntegration](./Caching/RedisMySQLIntegration/README.md) para observar cache distribuído com persistência.
5. [CacheIncrement](./Caching/CacheIncrement/README.md) para estudar escrita intensiva e sincronização.
6. [FusionCache](./Caching/FusionCache/README.md) para cenários de resiliência e stampede protection.
7. [UnifiedCacheSdk](./UnifiedCacheSdk/README.md) para padronização e reuso em soluções maiores.

### Dependências externas por cenário

| Cenário | Redis | MySQL |
|---------|-------|-------|
| Cache em memória local | Não | Não |
| Cache distribuído simples | Sim | Não |
| Cache distribuído com persistência | Sim | Sim |
| SDK com provedor configurável | Opcional | Não |

## Referências e documentação complementar

- [Microsoft Docs - Overview of caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/overview)
- [Redis documentation](https://redis.io/docs/)
- [FusionCache](https://github.com/ZiggyCreatures/FusionCache)
- [Caching/README.md](./Caching/README.md)
- [UnifiedCacheSdk/README.md](./UnifiedCacheSdk/README.md)