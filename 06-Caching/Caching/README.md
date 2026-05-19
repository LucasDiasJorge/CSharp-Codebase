# Caching - Padrões e Técnicas de Cache

## Visão geral

Coleção de exemplos práticos de estratégias de caching para .NET (ex.: .NET 10). A pasta reúne implementações didáticas e comparativas para estudar trade-offs entre memória local, cache distribuído e abordagens resilientes.

## Conceitos abordados

- **Cache-Aside**: Padrão lazy loading mais comum
- **Write-Through/Write-Behind**: Estratégias de escrita
- **Cache Distribuído**: Redis como camada de cache
- **Cache Híbrido**: Combinação de memória local + Redis
- **Resiliência**: Proteção contra cache stampede

## Objetivos de aprendizagem

- Identificar como os exemplos desta pasta cobrem estratégias de cache e integração com Redis.
- Escolher o subprojeto mais adequado para aprofundar o estudo.
- Reutilizar a navegação da pasta como índice prático de consulta.

## Estrutura da pasta `Caching`

```text
Caching/
+-- CacheAside/
|   +-- Controllers/
|   +-- Data/
|   +-- Interfaces/
|   +-- Models/
|   +-- Properties/
|   +-- Repositories/
|   +-- Services/
|   +-- appsettings.Development.json
|   \-- ...
+-- CacheIncrement/
|   +-- Controllers/
|   +-- Data/
|   +-- Interfaces/
|   +-- Models/
|   +-- Properties/
|   +-- Services/
|   +-- appsettings.Development.json
|   +-- appsettings.json
|   \-- ...
+-- CachePatterns/
|   +-- .vscode/
|   +-- Data/
|   +-- Models/
|   +-- Patterns/
|   +-- appsettings.json
|   +-- CachePatterns.csproj
|   +-- CachePatterns.sln
|   \-- Program.cs
+-- FusionCache/
|   +-- FusionCache.csproj
|   \-- Program.cs
+-- RedisConsoleApp/
|   +-- Program.cs
|   \-- RedisConsoleApp.csproj
+-- RedisHashFieldExpire/
|   +-- Program.cs
|   \-- RedisHashFieldExpire.csproj
\-- RedisMySQLIntegration/
    +-- Program.cs
    \-- RedisMySQLIntegration.csproj
```

## Como executar (exemplos)

Execute a partir da raiz do repositório (`06-Caching`):

```bash
dotnet run --project Caching/CacheAside/CacheAside.csproj
dotnet run --project Caching/CacheIncrement/CacheIncrement.csproj
dotnet run --project Caching/CachePatterns/CachePatterns.csproj
dotnet run --project Caching/FusionCache/FusionCache.csproj
dotnet run --project Caching/RedisConsoleApp/RedisConsoleApp.csproj
dotnet run --project Caching/RedisHashFieldExpire/RedisHashFieldExpire.csproj
dotnet run --project Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj
```

## Boas práticas e pontos de atenção

- **Chaves estruturadas**: Use prefixos (`product:123`, `user:session:abc`).
- **TTL apropriado**: Dados estáticos (horas), dados dinâmicos (minutos).
- **Invalidação seletiva**: Remova apenas chaves afetadas.
- **Monitoramento**: Acompanhe hit/miss ratio.
- **Fallback**: Sempre tenha plano B quando cache falhar.

## Projetos disponíveis

| Projeto | Descrição | Tecnologias |
|---------|-----------|-------------|
| [CacheAside](./CacheAside) | Padrão Cache-Aside com IMemoryCache | ASP.NET Core |
| [CacheIncrement](./CacheIncrement) | Contadores atômicos com sync para MySQL | Redis + MySQL |
| [CachePatterns](./CachePatterns) | Comparativo de estratégias | Console App |
| [FusionCache](./FusionCache) | Cache resiliente anti-stampede | FusionCache |
| [RedisConsoleApp](./RedisConsoleApp) | Operações essenciais do Redis | StackExchange.Redis |
| [RedisHashFieldExpire](./RedisHashFieldExpire) | Expiração por campo em hash (HEXPIRE) | StackExchange.Redis |
| [RedisMySQLIntegration](./RedisMySQLIntegration) | Cache distribuído + persistência | Redis + MySQL |

## Pré-requisitos

- .NET 10 SDK
- Redis (para projetos com cache distribuído)
- MySQL (para projetos com persistência)

## Referências

- [Microsoft Docs - Caching](https://docs.microsoft.com/aspnet/core/performance/caching/)
- [Redis Patterns](https://redis.io/topics/patterns)
- [FusionCache GitHub](https://github.com/ZiggyCreatures/FusionCache)
