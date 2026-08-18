# CLAUDE.md — AtomicOperationsDemo

Console sobre atomicidade sob concorrência, com Redis e SQLite como alvos reais. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/AtomicOperationsDemo/src/AtomicOperationsDemo/AtomicOperationsDemo.csproj
dotnet run --project 02-AsyncAndConcurrency/AtomicOperationsDemo/src/AtomicOperationsDemo/AtomicOperationsDemo.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`), mas com dependências de infraestrutura reais: `StackExchange.Redis` (operações atômicas server-side, como `INCR`) e EF Core + SQLite (transação/concorrência no banco). O contraste incremento não-atômico em memória versus operação atômica no store é o ponto do exemplo.

## Pontos de atenção

- **Exige Redis ativo** para a parte distribuída:

  ```bash
  docker run -d --name redis -p 6379:6379 redis
  ```

  Sem Redis, a execução falha na conexão. O SQLite é criado em arquivo local, sem serviço externo.
- TFM `net9.0`. Pacotes: `StackExchange.Redis` 2.7.33, `Microsoft.EntityFrameworkCore(.Sqlite)` 9.0.5.
- **Não há README local** neste diretório — o README fica na raiz do sample, em `02-AsyncAndConcurrency/AtomicOperationsDemo/`. Layout `src/`: aponte comandos sempre para o `.csproj` interno.
