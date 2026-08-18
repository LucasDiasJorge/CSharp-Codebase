# CLAUDE.md — State

Console: ciclo de vida de um pedido modelado como máquina de estados. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Behavioral/State/State.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/State/State.csproj
```

## Estrutura interna

- `States/IOrderState.cs` — contrato das transições.
- `States/{NewOrderState,PaidOrderState,ShippedOrderState,DeliveredOrderState,CancelledOrderState}.cs` — **cada estado é uma classe** que sabe para onde pode ir e recusa o resto.
- `Order.cs` — delega o comportamento ao estado atual em vez de decidir por `if`/`switch`.

O ganho: adicionar um estado é adicionar uma classe, sem tocar em condicional espalhada. Ao estender, mantenha a transição inválida sendo recusada **dentro** do estado — centralizá-la em `Order` desfaz o padrão.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Compare com `08-ArchitecturalPatterns/SagaPattern`, que também coordena etapas de pedido, mas com compensação distribuída.
