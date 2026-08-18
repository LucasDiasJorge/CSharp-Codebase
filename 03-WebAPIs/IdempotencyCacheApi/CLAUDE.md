# CLAUDE.md — IdempotencyCacheApi

API que aplica chave de idempotência em requisições de escrita. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/IdempotencyCacheApi/IdempotencyCacheApi.csproj
dotnet run --project 03-WebAPIs/IdempotencyCacheApi/IdempotencyCacheApi.csproj
```

Requisições de exemplo em `IdempotencyCacheApi.http` — repita o mesmo POST com a mesma chave para ver o efeito.

## Estrutura interna

- `Services/IdempotencyService.cs` (+ `IIdempotencyService`) — o núcleo. Guarda o resultado da primeira execução por chave e devolve o mesmo resultado em repetições, em vez de reprocessar.
- `Models/IdempotencyExecutionStatus.cs` / `IdempotencyExecutionResult.cs` — distinguem os três desfechos que importam: **primeira execução**, **repetição com mesmo payload** (devolve cache) e **repetição com payload diferente para a mesma chave** (conflito). Esse terceiro caso é o detalhe que separa uma implementação séria de uma ingênua.
- `Models/IdempotencyCacheEntry.cs` / `IdempotencyCacheOptions.cs` — entrada armazenada e TTL.
- `Services/PaymentProcessor.cs` — efeito colateral simulado que **não** deve repetir.

## Pontos de atenção

- TFM `net9.0`. Pacote: `Microsoft.AspNetCore.OpenApi` 9.0.11.
- O cache é **em memória, por processo**: não sobrevive a restart nem funciona com múltiplas instâncias. Em produção seria Redis — ver a trilha `06-Caching`.
