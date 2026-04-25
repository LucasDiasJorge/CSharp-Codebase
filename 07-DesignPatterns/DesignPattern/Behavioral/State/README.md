# State Design Pattern

## Visão geral

Projeto didático do CSharp-101 dedicado a State Design Pattern, com foco em design patterns, modelagem OO e código limpo.

## Conceitos abordados

- Exemplo didático sobre State Design Pattern no contexto de design patterns, modelagem OO e código limpo.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como State Design Pattern se aplica em um cenário prático de design patterns, modelagem OO e código limpo.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
State/
+-- States/
|   +-- CancelledOrderState.cs
|   +-- DeliveredOrderState.cs
|   +-- IOrderState.cs
|   +-- NewOrderState.cs
|   +-- PaidOrderState.cs
|   \-- ShippedOrderState.cs
+-- Order.cs
+-- Program.cs
\-- State.csproj
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/State/State.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Overview

The State pattern is a behavioral design pattern that allows an object to alter its behavior when its internal state changes. The object will appear to change its class.

##### Implementation

This implementation demonstrates the State pattern using an Order processing system:

1. **Context (Order)**: Maintains a reference to a concrete state object and delegates state-specific behavior to it.
2. **State Interface (IOrderState)**: Defines an interface for encapsulating behavior associated with a particular state of the Order.
3. **Concrete States**:
   - **NewOrderState**: Initial state of an order when it's created but not yet paid.
   - **PaidOrderState**: State after payment has been processed successfully.
   - **ShippedOrderState**: State after the order has been shipped.
   - **DeliveredOrderState**: State after the order has been delivered to the customer.
   - **CancelledOrderState**: State when an order has been cancelled.

##### When to Use

- When an object's behavior depends on its state and must change at runtime according to that state
- When operations have large, multipart conditional statements that depend on the object's state
- When state transitions are explicit and complex

##### Benefits

- Eliminates conditional statements by encapsulating state-specific behavior in separate classes
- Simplifies the code in the context (Order class)
- Makes state transitions explicit
- Allows new states to be added without changing existing state classes or the context

##### Structure

```
Order (Context)
 ↑
 |
 +--> IOrderState (State Interface)
       ↑
       |
       +---> NewOrderState
       |
       +---> PaidOrderState
       |
       +---> ShippedOrderState
       |
       +---> DeliveredOrderState
       |
       +---> CancelledOrderState
```

##### Example Usage

The demo in `Program.cs` shows how an Order object transitions through different states and how its behavior changes accordingly.
