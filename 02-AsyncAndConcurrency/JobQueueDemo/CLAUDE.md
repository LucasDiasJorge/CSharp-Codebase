# CLAUDE.md — JobQueueDemo

Console de fila de processamento concorrente com `System.Threading.Channels`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/JobQueueDemo/JobQueueDemo.csproj
dotnet run --project 02-AsyncAndConcurrency/JobQueueDemo/JobQueueDemo.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`) construído em níveis progressivos: produtor único → múltiplos consumidores → canal limitado (backpressure) → encerramento ordenado via `Complete()` e drenagem. `Channel<T>` é a primitiva central; não há biblioteca de fila externa.

## Pontos de atenção

- **BUILD QUEBRADO.** O `.csproj` não declara `TargetFramework` — dependia do `Directory.Build.props` removido no commit `50763d5`. `dotnet build` falha com `NETSDK1013`. Corrija declarando `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>` e `<ImplicitUsings>enable</ImplicitUsings>` no `.csproj`, ou restaure o props na raiz (conserta os 10 projetos afetados de uma vez — lista no [CLAUDE.md](../../CLAUDE.md) raiz).
- `System.Threading.Channels` faz parte do runtime; nenhum `PackageReference` é necessário.
