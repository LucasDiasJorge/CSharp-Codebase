# CLAUDE.md — CachePatterns

Console que implementa e compara **oito** estratégias de cache. É o mapa da trilha. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 06-Caching/Caching/CachePatterns/CachePatterns.csproj
dotnet run --project 06-Caching/Caching/CachePatterns/CachePatterns.csproj
```

## Estrutura interna

`Patterns/` — um arquivo por estratégia, todos sobre o mesmo `Data/Repository.cs`, o que torna a comparação honesta:

| Arquivo | Quem escreve no cache | Quando |
|---|---|---|
| `CacheAsidePattern` | aplicação | no miss |
| `ReadThroughPattern` | camada de cache | no miss, transparente |
| `WriteThroughPattern` | camada de cache | junto com a escrita |
| `WriteBehindPattern` | camada de cache | assíncrono, após a escrita |
| `RefreshAheadPattern` | background | antes de expirar |
| `FullCachePattern` | carga inicial | tudo em memória |
| `NearCachePattern` | local + remoto | por proximidade |
| `TieredCachePattern` | multi-nível | L1 memória, L2 distribuído |

Ao adicionar uma estratégia, crie novo arquivo em `Patterns/` sobre o mesmo repositório e registre no `Program.cs` — não altere os existentes.

## Pontos de atenção

- **BUILD QUEBRADO.** Sem `TargetFramework` no `.csproj`. O README afirma ".NET 9 via props global", mas esse `Directory.Build.props` **foi removido** no commit `50763d5` — a afirmação está desatualizada e o build falha com `NETSDK1013`. Pacotes são 8.0.x.
- Console puro com host manual (`Microsoft.Extensions.DependencyInjection` + `Logging` + `Configuration`), sem ASP.NET.
- Redis é opcional conforme a estratégia executada; os padrões em memória rodam sem serviço externo.
