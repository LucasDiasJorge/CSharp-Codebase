# CLAUDE.md — LogicalOperatorsDemo

Console que percorre os operadores lógicos básicos (`&&`, `||`, `!`, `^`). Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/LogicalOperatorsDemo/LogicalOperatorsDemo.csproj
dotnet run --project 01-Fundamentals/LogicalOperatorsDemo/LogicalOperatorsDemo.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): cada operador é demonstrado em bloco próprio com tabela-verdade impressa no console.

## Pontos de atenção

- **BUILD QUEBRADO.** O `.csproj` não declara `TargetFramework` — ele contava com o `Directory.Build.props` que foi removido do repositório no commit `50763d5`. Qualquer `dotnet build` aqui falha com:

  ```
  error NETSDK1013: O valor '' do TargetFramework não foi reconhecido.
  ```

  Correção: declarar `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>` e `<ImplicitUsings>enable</ImplicitUsings>` no `.csproj` (ou restaurar o `Directory.Build.props` na raiz, o que conserta os 10 projetos afetados de uma vez). Ver a lista completa no [CLAUDE.md](../../CLAUDE.md) raiz.
- Complementar a `ShortCircuitEvaluationDemo`, que cobre `&&`/`||` versus `&`/`|`. Evite duplicar aquele conteúdo aqui.
