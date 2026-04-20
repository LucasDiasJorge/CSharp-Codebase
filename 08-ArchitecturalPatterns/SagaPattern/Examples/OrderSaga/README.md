# Order Saga - Orchestration Pattern

## Visão geral

Projeto didático do CSharp-101 dedicado a Order Saga - Orchestration Pattern, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Order Saga - Orchestration Pattern no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Order Saga - Orchestration Pattern se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
OrderSaga/
+-- Context/
|   \-- OrderSagaContext.cs
+-- Steps/
|   +-- CreateOrderStep.cs
|   +-- CreateShipmentStep.cs
|   +-- ProcessPaymentStep.cs
|   \-- ReserveStockStep.cs
\-- OrderSagaOrchestrator.cs
```

## Como executar

```bash
cd SagaPattern

# Apenas orquestração
dotnet run -- orchestration
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

Implementação do padrão Saga usando **Orquestração**, onde um componente central (orquestrador) coordena a execução de todos os passos.

##### Fluxo

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

##### Passos da Saga

| Passo | Ação | Compensação |
|-------|------|-------------|
| CreateOrder | Criar pedido no sistema | Cancelar pedido |
| ReserveStock | Reservar estoque | Liberar estoque |
| ProcessPayment | Processar pagamento | Estornar pagamento |
| CreateShipment | Criar ordem de envio | Cancelar envio |

##### Estrutura

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

##### Exemplo de Uso

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
