# CLAUDE.md — Asynchronous

Console introdutório de `async`/`await`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/Asynchronous/Asynchronous.csproj
dotnet run --project 02-AsyncAndConcurrency/Asynchronous/Asynchronous.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`) com blocos sequenciais: método assíncrono básico, `await` de operação demorada, sequencial versus paralelo, e propagação de exceção através de `await`.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas — a "operação demorada" é `Task.Delay`, não I/O real.
- É o exemplo de entrada da trilha. `AsyncTasksDemo` cobre o mesmo terreno com mais profundidade; ao editar, evite que os dois convirjam para o mesmo conteúdo.
