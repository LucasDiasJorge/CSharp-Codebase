# CLAUDE.md — Threads

Console sobre `Thread` e sincronização de baixo nível. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/Threads/Threads.csproj
dotnet run --project 02-AsyncAndConcurrency/Threads/Threads.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): criação e `Join` de threads, condição de corrida demonstrada de forma observável, e sua correção com `lock`. O valor didático está no contraste com a trilha `async`/`await` — aqui o paralelismo é por thread dedicada, não por continuação sobre thread pool.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- A demonstração de condição de corrida é **não-determinística por natureza**: pode não reproduzir em toda execução. Isso é esperado; não a "conserte" com sincronização, ou o exemplo perde a função.
