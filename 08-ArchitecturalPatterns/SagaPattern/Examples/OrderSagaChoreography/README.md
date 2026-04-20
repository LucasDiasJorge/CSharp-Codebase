# Order Saga - Choreography Pattern

## Visão geral

Projeto didático do CSharp-101 dedicado a Order Saga - Choreography Pattern, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Order Saga - Choreography Pattern no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Order Saga - Choreography Pattern se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
OrderSagaChoreography/
+-- Events/
|   +-- OrderCreated.cs
|   +-- PaymentProcessed.cs
|   +-- SagaCompleted.cs
|   +-- SagaFailed.cs
|   +-- ShipmentCreated.cs
|   +-- StartOrderSaga.cs
|   \-- StockReserved.cs
+-- Handlers/
|   +-- InventoryServiceHandler.cs
|   +-- OrderServiceHandler.cs
|   +-- PaymentServiceHandler.cs
|   \-- ShippingServiceHandler.cs
+-- Messaging/
|   +-- IEventBus.cs
|   \-- InMemoryEventBus.cs
\-- OrderSagaChoreographyRunner.cs
```

## Como executar

```bash
cd SagaPattern

# Apenas coreografia
dotnet run -- choreography
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

Implementação do padrão Saga usando **Coreografia (Choreography)**, onde não existe um orquestrador central.
Cada “serviço” reage a eventos e publica novos eventos para disparar o próximo passo.

##### Fluxo

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

## Referências

- [Microservices Patterns: Saga Pattern](https://microservices.io/patterns/data/saga.html)
