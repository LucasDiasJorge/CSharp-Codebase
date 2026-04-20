# Caching - Padrões e Técnicas de Cache

## Visão geral

Coleção de exemplos práticos de estratégias de caching para .NET 9.

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

## Estrutura do projeto

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
\-- RedisMySQLIntegration/
    +-- Program.cs
    \-- RedisMySQLIntegration.csproj
```

## Como executar

Escolha um dos projetos abaixo para execução direcionada:

- `dotnet run --project 06-Caching/Caching/CacheAside/CacheAside.csproj`
- `dotnet run --project 06-Caching/Caching/CacheIncrement/CacheIncrement.csproj`
- `dotnet run --project 06-Caching/Caching/CachePatterns/CachePatterns.csproj`
- `dotnet run --project 06-Caching/Caching/FusionCache/FusionCache.csproj`
- `dotnet run --project 06-Caching/Caching/RedisConsoleApp/RedisConsoleApp.csproj`
- `dotnet run --project 06-Caching/Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj`

## Boas práticas e pontos de atenção

- **Chaves estruturadas**: Use prefixos (`product:123`, `user:session:abc`)
- **TTL apropriado**: Dados estáticos (horas), dados dinâmicos (minutos)
- **Invalidação seletiva**: Remova apenas chaves afetadas
- **Monitoramento**: Acompanhe hit/miss ratio
- **Fallback**: Sempre tenha plano B quando cache falhar

## Conteúdo complementar

##### Projetos Disponíveis

| Projeto | Descrição | Tecnologias |
|---------|-----------|-------------|
| [CacheAside](./CacheAside) | Padrão Cache-Aside com IMemoryCache | ASP.NET Core |
| [CacheIncrement](./CacheIncrement) | Contadores atômicos com sync para MySQL | Redis + MySQL |
| [CachePatterns](./CachePatterns) | Comparativo de 8 estratégias | Console App |
| [FusionCache](./FusionCache) | Cache resiliente anti-stampede | FusionCache |
| [RedisConsoleApp](./RedisConsoleApp) | Operações essenciais do Redis | StackExchange.Redis |
| [RedisMySQLIntegration](./RedisMySQLIntegration) | Cache distribuído + persistência | Redis + MySQL |

##### Quando Usar Cada Projeto

| Cenário | Projeto Recomendado |
|---------|---------------------|
| Aprender o básico de cache em APIs | **CacheAside** |
| Contadores/métricas de alto tráfego | **CacheIncrement** |
| Entender diferentes padrões e escolher | **CachePatterns** |
| Necessita resiliência e anti-stampede | **FusionCache** |
| Explorar recursos avançados do Redis | **RedisConsoleApp** |
| Cache distribuído simples com banco | **RedisMySQLIntegration** |

##### Pré-requisitos

- .NET 9.0 SDK
- Redis (para projetos com cache distribuído)
- MySQL (para projetos com persistência)

##### Instalação de Dependências (Docker)

```bash
# Redis
docker run -d --name redis -p 6379:6379 redis

# MySQL
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=senha123 -p 3306:3306 mysql:8
```

##### Cache-Aside (Lazy Loading)

```
App → Verifica Cache → Miss → Busca DB → Popula Cache → Retorna
```

##### Write-Through

```
App → Escreve Cache + DB (síncrono)
```

##### Write-Behind

```
App → Escreve Cache → Background Job → Persiste DB
```

##### Ordem de Estudo Recomendada

1. **CacheAside** → Fundamentos e padrão mais comum
2. **CachePatterns** → Visão comparativa de estratégias
3. **RedisConsoleApp** → Recursos do Redis
4. **RedisMySQLIntegration** → Cache distribuído básico
5. **CacheIncrement** → Padrão de alta performance
6. **FusionCache** → Resiliência e recursos avançados

## Referências

- [Microsoft Docs - Caching](https://docs.microsoft.com/aspnet/core/performance/caching/)
- [Redis Patterns](https://redis.io/topics/patterns)
- [FusionCache GitHub](https://github.com/ZiggyCreatures/FusionCache)
