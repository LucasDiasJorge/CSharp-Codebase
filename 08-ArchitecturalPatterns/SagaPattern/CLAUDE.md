# CLAUDE.md — SagaPattern

Console: transação distribuída nas **duas** variantes — orquestração e coreografia. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/SagaPattern/SagaPattern.csproj
dotnet run --project 08-ArchitecturalPatterns/SagaPattern/SagaPattern.csproj
```

## Estrutura interna

O valor do projeto está em ter as duas abordagens sobre o mesmo fluxo de pedido:

**Orquestração** — `Core/` (`ISagaOrchestrator`, `ISagaStep`, `SagaOrchestrator`, `SagaState`, `SagaResult`) + `Examples/OrderSaga/Steps/` (`CreateOrderStep`, `ReserveStockStep`, `ProcessPaymentStep`, `CreateShipmentStep`). Um coordenador central conhece a sequência e dispara as **compensações** em ordem inversa quando um passo falha.

**Coreografia** — `Examples/OrderSagaChoreography/` com `Events/` e `Handlers/` (`OrderService`, `Inventory`, `Payment`, `Shipping`). Não há coordenador: cada serviço reage a um evento e publica o próximo. O fluxo existe apenas como consequência emergente.

Cada `ISagaStep` precisa do seu passo de compensação — é o que torna a saga reversível sem transação distribuída de verdade.

## Pontos de atenção

- TFM `net8.0`, sem dependências externas; event bus em memória.
- É o maior projeto da trilha (26 arquivos). Ao estender a orquestração, adicione o step **e** sua compensação; um step sem compensação quebra o rollback silenciosamente.
