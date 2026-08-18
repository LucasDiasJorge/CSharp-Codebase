# CLAUDE.md — sqlite-sample-api.Tests

Testes xUnit de `sqlite-sample-api`. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet test 09-Data/Data/sqlite-sample-api.Tests/sqlite-sample-api.Tests.csproj

# um teste ou subconjunto:
dotnet test 09-Data/Data/sqlite-sample-api.Tests/sqlite-sample-api.Tests.csproj --filter "FullyQualifiedName~BooksController"
```

## Estrutura interna

Duas estratégias separadas por pasta, e a distinção é o ponto:

- `Unit/AuthorsControllerUnitTests.cs` e `Unit/BooksControllerUnitTests.cs` — usam **EF Core InMemory**: rápidos, isolados, mas o provider não valida SQL nem constraints reais.
- `Integration/SqliteDatabaseIntegrationTests.cs` — usam **SQLite de verdade**, pegando o que o InMemory deixa passar.

Ao adicionar teste, escolha a pasta pela estratégia, não pela classe testada.

## Pontos de atenção

- **TFM `net6.0`**, alinhado ao projeto sob teste. `Microsoft.NET.Test.Sdk` 17.12.0 e xUnit 2.9.2 são bem mais novos que o TFM — funciona, mas é uma combinação incomum.
- **Sem README local**; documentação em `09-Data/Data/sqlite-sample-api/`.
- É um dos **três** projetos de teste do repositório (com `12-Testing/OrderRuleConsole.Tests` e `13-SDKsAndLibraries/MySimpleSdk.Tests`).
