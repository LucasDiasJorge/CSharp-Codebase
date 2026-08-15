# CLAUDE.md — OrderRuleConsole.Tests

Testes xUnit do motor de regras. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet test 12-Testing/OrderRuleConsole/OrderRuleConsole.Tests/OrderRuleConsole.Tests.csproj

# um teste ou subconjunto:
dotnet test 12-Testing/OrderRuleConsole/OrderRuleConsole.Tests/OrderRuleConsole.Tests.csproj --filter "FullyQualifiedName~OrderRuleEngineTests"
```

## Estrutura interna

`OrderRuleEngineTests.cs` — cobre `Services/OrderRuleEngine.cs` do projeto pai, referenciado via `ProjectReference`. `<Using Include="Xunit" />` no `.csproj` dispensa o `using Xunit;` nos arquivos de teste.

## Pontos de atenção

- Este projeto está **aninhado dentro do diretório do projeto testado**, layout incomum. Funciona porque o `.csproj` pai exclui esta pasta da compilação (`<Compile Remove="OrderRuleConsole.Tests\**\*.cs" />`). Se o build do pai começar a reclamar de xUnit ou de atributos de assembly duplicados, foi essa exclusão que se perdeu.
- **Sem README local**; documentação em `12-Testing/OrderRuleConsole/`.
- TFM `net9.0`. xUnit 2.9.2, `Microsoft.NET.Test.Sdk` 17.12.0, `coverlet.collector` 6.0.4.
- É o projeto de teste de referência do repositório — dos três existentes, é o único totalmente saudável e em TFM atual.
