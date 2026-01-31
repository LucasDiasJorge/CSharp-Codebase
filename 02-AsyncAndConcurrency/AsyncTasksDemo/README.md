# 🔄 Async Tasks Demo

Demonstração de padrões fundamentais de `async/await` em .NET 9.

---

## 📚 Conceitos Abordados

- **Task.Delay**: Atraso não-bloqueante para entender o fluxo assíncrono
- **Task.WhenAll**: Execução paralela de múltiplas tarefas
- **Task.WhenAny**: Aguardar a primeira tarefa completada
- **Background Tasks**: Tarefas com intervalo e finalização graciosa
- **AggregateException**: Tratamento de exceções em tarefas paralelas

---

## 🎯 Objetivos de Aprendizado

- Entender a diferença entre iniciar e aguardar tarefas
- Demonstrar coordenação com `Task.WhenAll` e `Task.WhenAny`
- Evidenciar captura de exceções agregadas
- Compreender execução paralela controlada

---

## 📂 Estrutura do Projeto

```
AsyncTasksDemo/
├── Program.cs        # Demonstrações práticas
└── README.md
```

---

## 🚀 Como Executar

```bash
cd AsyncTasksDemo
dotnet run
```

---

## 💡 Exemplos de Código

### Task.WhenAll - Execução Paralela

```csharp
Task<string> task1 = FetchApiAsync("API 1", 2000);
Task<string> task2 = FetchApiAsync("API 2", 1500);

string[] results = await Task.WhenAll(task1, task2);
// Ambas executam em paralelo, aguarda todas completarem
```

### Task.WhenAny - Primeira Completada

```csharp
Task<string> completed = await Task.WhenAny(task1, task2);
string result = await completed;
// Retorna assim que a primeira terminar
```

---

## 📋 Métodos Principais

| Método | Descrição |
|--------|-----------|
| `PerformTaskAsync` | Simula operação longa com delay não-bloqueante |
| `ParallelApiCallsAsync` | Executa duas tarefas em paralelo com `WhenAll` |
| `StartLoggingAsync` | Logging periódico com suporte a cancelamento |

---

## 📝 Saída Esperada

```
[Example 1] Basic asynchronous task (non-blocking delay)
Task starting...
Processing...
Task finished.
Task completed!

[Example 2] Parallel API simulation with Task.WhenAll
Fetching API 1...
Fetching API 2...
API 2 Data Retrieved.
API 1 Data Retrieved.
All API calls completed.
```

---

## ✅ Boas Práticas

- Tipagem explícita (sem `var`) para clareza didática
- Uso adequado de `await` para não bloquear threads
- Organização do fluxo assíncrono em métodos separados

---

## ⚠️ Pontos de Atenção

- Nunca usar `.Result` ou `.Wait()` para evitar deadlocks
- Sempre propagar `CancellationToken` quando disponível
- Tratar exceções com try/catch em tarefas paralelas

---

## 🔜 Próximos Passos

- Adicionar `CancellationToken` para cancelamento gracioso
- Comparar com `Parallel.ForEachAsync`
- Medir tempo de execução com `Stopwatch`
- Adicionar `IProgress<T>` para reportar progresso

---

## 🔗 Referências

- [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
