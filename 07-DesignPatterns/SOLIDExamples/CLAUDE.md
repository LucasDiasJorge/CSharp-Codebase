# CLAUDE.md — SOLIDExamples

Console com os cinco princípios SOLID, cada um em comparação incorreto versus correto. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/SOLIDExamples/SOLIDExamples.csproj
dotnet run --project 07-DesignPatterns/SOLIDExamples/SOLIDExamples.csproj
```

## Estrutura interna

Uma pasta por princípio, com o arquivo homônimo dentro: `SRP/SrpExample.cs`, `OCP/OcpExample.cs`, `LSP/LspExample.cs`, `ISP/IspExample.cs`, `DIP/DipExample.cs`. `Program.cs` executa todos em sequência.

Cada arquivo carrega **as duas versões** — a que viola e a que respeita o princípio — lado a lado. Ao editar, preserve esse pareamento: remover o lado ruim apaga a lição.

## Pontos de atenção

- **BUILD QUEBRADO.** O `.csproj` não declara `TargetFramework` (dependia do `Directory.Build.props` removido no commit `50763d5`); `dotnet build` falha com `NETSDK1013`. Declare `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>` e `<ImplicitUsings>enable</ImplicitUsings>`, ou restaure o props na raiz — lista dos 10 afetados no [CLAUDE.md](../../CLAUDE.md) raiz.
- Sem dependências externas.
