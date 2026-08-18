# CLAUDE.md — CacheAside

API que implementa o padrão **Cache-Aside** (lazy loading) com camadas explícitas. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 06-Caching/Caching/CacheAside/CacheAside.csproj
dotnet run --project 06-Caching/Caching/CacheAside/CacheAside.csproj
```

Requisições de exemplo em `CacheAside.http`.

## Estrutura interna

O padrão exige que **a aplicação**, e não a infraestrutura, decida sobre o cache. A separação reflete isso:

- `Interfaces/ICacheService.cs` + `Services/MemoryCacheService.cs` — acesso ao cache.
- `Interfaces/IProductRepository.cs` + `Repositories/ProductRepository.cs` — acesso aos dados (EF Core InMemory).
- `Services/ProductService.cs` — **onde o padrão vive**: consulta o cache, em caso de miss vai ao repositório, popula o cache e devolve. Nem o repositório nem o cache conhecem essa lógica.
- `Controllers/CacheController.cs` — permite inspecionar/invalidar o cache e observar hit versus miss.

## Pontos de atenção

- **BUILD QUEBRADO.** Sem `TargetFramework` no `.csproj` (dependia do `Directory.Build.props` removido no commit `50763d5`); falha com `NETSDK1013`. Os pacotes são da linha **8.0.x**, então `net8.0` é o alvo coerente ao corrigir. Ver lista completa dos 10 projetos afetados no [CLAUDE.md](../../../CLAUDE.md) raiz.
- `Microsoft.Extensions.Caching.StackExchangeRedis` está referenciado, mas a implementação ativa é `MemoryCacheService` — **não é preciso Redis** para rodar.
- Contraste com `CachePatterns`, que compara oito estratégias lado a lado; aqui uma única é implementada a fundo.
