# CLAUDE.md — AsyncTasksDemo

Console com os padrões fundamentais de `Task` em .NET. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/AsyncTasksDemo/AsyncTasksDemo.csproj
dotnet run --project 02-AsyncAndConcurrency/AsyncTasksDemo/AsyncTasksDemo.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`) organizado por padrão: `Task.WhenAll` para paralelismo, `Task.WhenAny` para corrida/timeout, `CancellationToken` para cancelamento cooperativo e tratamento de `AggregateException`.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Sobreposição deliberada com `Asynchronous` (introdutório) e `TaskWhenAll/example` (foco só em `WhenAll`). Ao mexer, mantenha cada um no seu nível.
