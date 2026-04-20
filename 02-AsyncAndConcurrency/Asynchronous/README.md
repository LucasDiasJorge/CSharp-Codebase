# Programação Assíncrona em C#

## Visão geral

Projeto didático do CSharp-101 dedicado a Programação Assíncrona em C#, com foco em assincronia, tasks, threads e coordenação de trabalho.

## Conceitos abordados

Este projeto demonstra os fundamentos da programação assíncrona em C#, incluindo:

- **async/await**: Palavras-chave para programação assíncrona
- **Task**: Representação de operações assíncronas
- **CancellationToken**: Controle de cancelamento de operações
- **Task.WhenAll**: Execução paralela de múltiplas tarefas
- **Task.Run**: Execução de código em background

## Objetivos de aprendizagem

- Entender quando e por que usar programação assíncrona
- Aprender a diferença entre paralelismo e assincronismo
- Dominar o uso de async/await
- Controlar o ciclo de vida de tarefas assíncronas
- Implementar cancelamento de operações longas

### O que Você Aprenderá

1. **Por que usar async/await**: Evita bloqueio da thread principal
2. **Diferença entre Task.Run e async/await**: Task.Run para CPU-bound, async/await para I/O-bound
3. **Controle de cancelamento**: Como cancelar operações longas graciosamente
4. **Execução paralela**: Como executar múltiplas operações simultaneamente
5. **Tratamento de exceções**: Como lidar com erros em código assíncrono

## Estrutura do projeto

```text
Asynchronous/
+-- Asynchronous.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/Asynchronous/Asynchronous.csproj
```

## Boas práticas e pontos de atenção

- **Deadlock**: Evite usar `.Result` ou `.Wait()` em contextos síncronos
- **ConfigureAwait(false)**: Use em bibliotecas para melhor performance
- **Cancellation**: Sempre propague CancellationTokens quando possível
- **Exception Handling**: Use try/catch adequadamente com async/await

## Conteúdo complementar

##### Async/Await

```csharp
public static async Task<string> GetDataAsync()
{
    await Task.Delay(2000); // Simula operação I/O
    return "Dados obtidos!";
}
```

##### Paralelismo com Task.WhenAll

```csharp
Task<string> data1 = GetDataAsync();
Task<string> data2 = GetDataAsync();
await Task.WhenAll(data1, data2); // Executa em paralelo
```

##### Cancelamento com CancellationToken

```csharp
var cts = new CancellationTokenSource();
await Task.Delay(3000, cts.Token); // Operação cancelável
```

## Referências

- [Documentação oficial sobre async/await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
- [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
