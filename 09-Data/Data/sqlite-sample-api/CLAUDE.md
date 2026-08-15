# CLAUDE.md — sqlite-sample-api

Web API com EF Core + SQLite, no estilo **controllers clássico com `Startup.cs`**. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 09-Data/Data/sqlite-sample-api/sqlite-sample-api.csproj
dotnet run --project 09-Data/Data/sqlite-sample-api/sqlite-sample-api.csproj

# testes do projeto irmão:
dotnet test 09-Data/Data/sqlite-sample-api.Tests/sqlite-sample-api.Tests.csproj
```

## Estrutura interna

- `Program.cs` + **`Startup.cs`** — modelo pré-.NET 6, com `ConfigureServices`/`Configure` separados. É o único projeto do repositório assim, e é justamente o que o torna útil como referência de código legado.
- `Controllers/AuthorsController.cs`, `BooksController.cs` — CRUD tradicional.
- `Data/AppDbContext.cs` + `Data/DbInitializer.cs` — schema e **seed inicial**.
- `Dtos/` e `Models/` — separação entre contrato e entidade.

## Pontos de atenção

- **TFM `net6.0`** — fora de suporte, e todos os pacotes acompanham (EF Core 6, Swashbuckle 6.5.0). Preserve ao editar: o projeto de testes também está em `net6.0` e migrar um exige migrar o outro.
- SQLite em arquivo local: **sem serviço externo**, roda direto.
- É um dos três projetos com testes automatizados do repositório.
