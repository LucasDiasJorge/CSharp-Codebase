# Saga Pattern em C#

## O que é o Padrão Saga?

O **Saga Pattern** é um padrão para gerenciar transações distribuídas em arquiteturas de microserviços. Ele divide uma transação longa em uma sequência de transações locais, onde cada transação atualiza um serviço e publica eventos para disparar a próxima transação.

## Tipos de Saga

| Tipo | Descrição | Uso |
|------|-----------|-----|
| **Choreography** | Cada serviço reage a eventos | Sagas simples |
| **Orchestration** | Um orquestrador central coordena | Sagas complexas |

## Estrutura

```
SagaPattern/
├── Core/
│   ├── ISaga.cs
│   ├── ISagaStep.cs
│   ├── SagaState.cs
│   └── SagaResult.cs
├── Examples/
│   ├── OrderSaga/          # Orquestração
│   └── PaymentSaga/        # Coreografia
├── Program.cs
└── README.md
```

## Fluxo de uma Saga

```
┌────────┐    ┌────────┐    ┌────────┐    ┌────────┐
│ Step 1 │───▶│ Step 2 │───▶│ Step 3 │───▶│ Step 4 │
│ Create │    │ Reserve│    │ Payment│    │ Ship   │
│ Order  │    │ Stock  │    │        │    │        │
└────────┘    └────────┘    └────────┘    └────────┘
     │             │             │             │
     ▼             ▼             ▼             ▼
┌────────┐    ┌────────┐    ┌────────┐    ┌────────┐
│Compensate│◀──│Compensate│◀──│Compensate│◀──│   ✓    │
│ Cancel  │    │ Release │    │ Refund  │    │        │
└────────┘    └────────┘    └────────┘    └────────┘
```

## Quando Usar

✅ Transações distribuídas entre microserviços  
✅ Operações que precisam de rollback em múltiplos serviços  
✅ Processos de negócio que atravessam bounded contexts  

## Como Executar

```bash
cd SagaPattern
dotnet run
```
