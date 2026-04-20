# Saga Pattern em C#

## Visão geral

O **Saga Pattern** é um padrão para gerenciar transações distribuídas em arquiteturas de microserviços. Ele divide uma transação longa em uma sequência de transações locais, onde cada transação atualiza um serviço e publica eventos para disparar a próxima transação.

## Conceitos abordados

- Exemplo didático sobre Saga Pattern em C# no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Saga Pattern em C# se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
SagaPattern/
+-- Core/
|   +-- ISagaOrchestrator.cs
|   +-- ISagaStep.cs
|   +-- SagaOrchestrator.cs
|   +-- SagaResult.cs
|   \-- SagaState.cs
+-- Examples/
|   +-- OrderSaga/
|   \-- OrderSagaChoreography/
+-- SagaPattern/
+-- Program.cs
\-- SagaPattern.csproj
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/SagaPattern/SagaPattern.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Tipos de Saga

| Tipo | Descrição | Uso |
|------|-----------|-----|
| **Choreography** | Cada serviço reage a eventos | Sagas simples |
| **Orchestration** | Um orquestrador central coordena | Sagas complexas |

##### Estrutura

```
SagaPattern/
├── Core/
│   ├── ISagaStep.cs
│   ├── ISagaOrchestrator.cs
│   ├── SagaOrchestrator.cs
│   ├── SagaState.cs
│   └── SagaResult.cs
├── Examples/
│   ├── OrderSaga/                # Orchestration (orquestrador central)
│   └── OrderSagaChoreography/    # Choreography (event-driven)
├── Program.cs
└── README.md
```

##### Fluxo de uma Saga

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

##### Quando Usar

✅ Transações distribuídas entre microserviços  
✅ Operações que precisam de rollback em múltiplos serviços  
✅ Processos de negócio que atravessam bounded contexts
