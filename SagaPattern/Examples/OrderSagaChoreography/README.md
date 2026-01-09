# Order Saga - Choreography Pattern

## Descrição

Implementação do padrão Saga usando **Coreografia (Choreography)**, onde não existe um orquestrador central.
Cada “serviço” reage a eventos e publica novos eventos para disparar o próximo passo.

## Fluxo

```
StartOrderSaga
   │
   ▼
[Order] CreateOrder  ──► OrderCreated
   │                    │
   │                    ▼
   │              [Inventory] ReserveStock ──► StockReserved
   │                                            │
   │                                            ▼
   │                                  [Payment] ProcessPayment ──► PaymentProcessed
   │                                                              │
   │                                                              ▼
   │                                                    [Shipping] CreateShipment ──► SagaCompleted

Falha em qualquer etapa:
  └──► SagaFailed  (cada serviço que já executou compensa a si mesmo)
```

## Como executar

```bash
cd SagaPattern

# Apenas coreografia
dotnet run -- choreography
```
