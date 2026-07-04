# Task.WhenAll

## Visão geral

Demonstração prática de `Task.WhenAll` em .NET, comparando a execução **sequencial** de tarefas assíncronas (aguardando uma de cada vez) com a execução **concorrente** (disparando todas e aguardando o conjunto).

## Conceitos abordados

- **Execução sequencial**: `await` em cada tarefa individualmente, somando os tempos
- **Task.WhenAll**: disparo simultâneo de várias tarefas, aguardando todas ao mesmo tempo
- **Task.Delay**: atraso não-bloqueante para simular trabalho assíncrono
- **Medição de tempo**: comparação do tempo total com `DateTime`

## Objetivos de aprendizagem

- Entender a diferença entre aguardar tarefas em sequência e em paralelo
- Compreender que `Task.WhenAll` não bloqueia a thread enquanto as tarefas rodam
- Evidenciar o ganho de desempenho ao coordenar tarefas independentes
- Reforçar que o tempo total tende ao da tarefa mais longa, não à soma delas

## Estrutura do projeto

```text
TaskWhenAll/
\-- example/
    +-- example.csproj
    \-- Program.cs
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/TaskWhenAll/example/example.csproj
```

## Métodos principais

| Método | Descrição |
|--------|-----------|
| `Example.WaitOneTask` | Aguarda 1 segundo e sinaliza conclusão |
| `Example.WaitTwoTask` | Aguarda 2 segundos e sinaliza conclusão |
| `Example.WaitThreeTask` | Aguarda 3 segundos e sinaliza conclusão |

## Comparação

```csharp
// Sequencial: ~6s (1 + 2 + 3)
await Example.WaitOneTask();
await Example.WaitTwoTask();
await Example.WaitThreeTask();

// Concorrente: ~3s (tempo da mais longa)
await Task.WhenAll(
    Example.WaitOneTask(),
    Example.WaitTwoTask(),
    Example.WaitThreeTask()
);
```

## Saída esperada

```text
Starting tasks...
One task completed.
Two tasks completed.
Three tasks completed.
All tasks completed in ~6 seconds.
Starting tasks with Task.WhenAll...
One task completed.
Two tasks completed.
Three tasks completed.
All tasks completed in ~3 seconds.
```

> Os tempos são aproximados e podem variar conforme o agendamento do sistema.

## Boas práticas e pontos de atenção

- Use `Task.WhenAll` apenas para tarefas **independentes** entre si
- Nunca use `.Result` ou `.Wait()` para evitar deadlocks
- `Task.WhenAll` relança a primeira exceção; para tratar todas, inspecione `AggregateException`
- Propague `CancellationToken` quando disponível para cancelamento gracioso

## Próximos passos

- Retornar valores com `Task.WhenAll` sobre `Task<T>` (obtendo um array de resultados)
- Comparar com `Task.WhenAny` para reagir à primeira tarefa concluída
- Medir o tempo com `Stopwatch` em vez de `DateTime`
- Adicionar tratamento de exceções em tarefas paralelas

## Referências

- [Task.WhenAll](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall)
- [Task-based Asynchronous Pattern (TAP)](https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
