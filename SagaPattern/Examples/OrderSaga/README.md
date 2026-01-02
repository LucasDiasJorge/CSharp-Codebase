# Order Saga - Orchestration Pattern

## Descrição

Implementação do padrão Saga usando **Orquestração**, onde um componente central (orquestrador) coordena a execução de todos os passos.

## Fluxo

```
┌─────────────────────────────────────────────────────────────┐
│                    OrderSagaOrchestrator                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────┐   ┌────────────┐   ┌────────────┐   ┌─────┐ │
│  │CreateOrder │──▶│ReserveStock│──▶│ProcessPay- │──▶│Ship │ │
│  │            │   │            │   │   ment     │   │     │ │
│  └────────────┘   └────────────┘   └────────────┘   └─────┘ │
│        │               │                │              │     │
│        ▼               ▼                ▼              ▼     │
│  ┌────────────┐   ┌────────────┐   ┌────────────┐   ┌─────┐ │
│  │  Cancel    │◀──│  Release   │◀──│  Refund    │◀──│  -  │ │
│  │  Order     │   │  Stock     │   │            │   │     │ │
│  └────────────┘   └────────────┘   └────────────┘   └─────┘ │
│                                                              │
│                     COMPENSAÇÃO                              │
└─────────────────────────────────────────────────────────────┘
```

## Passos da Saga

| Passo | Ação | Compensação |
|-------|------|-------------|
| CreateOrder | Criar pedido no sistema | Cancelar pedido |
| ReserveStock | Reservar estoque | Liberar estoque |
| ProcessPayment | Processar pagamento | Estornar pagamento |
| CreateShipment | Criar ordem de envio | Cancelar envio |

## Estrutura

```
OrderSaga/
├── Context/
│   └── OrderSagaContext.cs
├── Steps/
│   ├── CreateOrderStep.cs
│   ├── ReserveStockStep.cs
│   ├── ProcessPaymentStep.cs
│   └── CreateShipmentStep.cs
├── OrderSagaOrchestrator.cs
└── README.md
```

## Exemplo de Uso

```csharp
// Cenário de sucesso
var orchestrator = new OrderSagaOrchestrator();
var context = new OrderSagaContext
{
    CustomerId = Guid.NewGuid(),
    Items = [new OrderItem { ProductId = Guid.NewGuid(), Quantity = 2 }],
    TotalAmount = 500.00m
};

var result = await orchestrator.ExecuteAsync(context);

if (result.IsSuccess)
{
    Console.WriteLine($"Pedido: {context.OrderId}");
    Console.WriteLine($"Tracking: {context.ShippingTrackingCode}");
}

// Cenário com falha no pagamento
var orchestratorWithFailure = new OrderSagaOrchestrator(paymentShouldFail: true);
var failResult = await orchestratorWithFailure.ExecuteAsync(context);
// Saga será compensada automaticamente
```
