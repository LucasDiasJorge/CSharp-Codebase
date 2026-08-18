# YieldReturnDemo

Console app didático que explica `yield return` em C#: o que o compilador gera, por que a execução é preguiçosa e quais armadilhas aparecem no uso real.

## Visão geral

Um método que contém `yield return` deixa de ser um método comum e vira um **iterador**. O compilador reescreve o corpo como uma máquina de estados que implementa `IEnumerable<T>`/`IEnumerator<T>`: nada executa na chamada, e cada `MoveNext()` do consumidor roda o corpo até o próximo `yield return`, pausa e devolve o valor. A execução retoma exatamente de onde parou na próxima iteração.

O projeto roda seis seções em sequência, da mais simples para a mais avançada. A seção 1 mostra apenas a sintaxe, sem explicações internas: é por onde começar. A seção 2 é a mais importante do conjunto, porque imprime o intercalamento entre `[iterador]` e `[consumidor]` e revela a mecânica que sustenta todas as demais. Ao ler o código, observe onde a execução pausa e o que acontece quando o `foreach` é interrompido antes do fim.

## Conceitos abordados

- Sintaxe básica de um método iterador e a diferença para um método que devolve `List<T>`.
- Execução adiada (deferred execution) e reexecução do iterador a cada nova enumeração.
- Máquina de estados gerada pelo compilador, comparada com uma implementação manual de `IEnumerator<T>`.
- Sequências infinitas e `yield break` como saída antecipada.
- Composição de operadores lazy em pipeline, com processamento item a item.
- Diferença de alocação entre materializar a coleção (eager) e produzir sob demanda (lazy).
- Armadilhas: múltiplas enumerações, validação de argumentos adiada e liberação de recursos com `try/finally`.

## Objetivos de aprendizagem

- Escrever um método iterador simples e consumi-lo com `foreach`.
- Explicar por que o corpo de um iterador não executa no momento da chamada.
- Reconhecer quando `yield return` reduz memória e trabalho desnecessário.
- Implementar operadores próprios no estilo LINQ usando iteradores.
- Evitar a reexecução acidental de consultas caras decidindo quando materializar com `ToList()`.
- Separar método público validador de iterador privado para que exceções de argumento sejam lançadas na hora certa.

## Estrutura do projeto

```text
YieldReturnDemo/
|-- Demos/
|   |-- BasicsDemo.cs
|   |-- DeferredExecutionDemo.cs
|   |-- ManualEnumeratorDemo.cs
|   |-- InfiniteSequenceDemo.cs
|   |-- StreamingPipelineDemo.cs
|   `-- PitfallsDemo.cs
|-- LazyOperators.cs
|-- Program.cs
`-- YieldReturnDemo.csproj
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/YieldReturnDemo/YieldReturnDemo.csproj
```

Somente compilar:

```bash
dotnet build 01-Fundamentals/YieldReturnDemo/YieldReturnDemo.csproj
```

## Boas práticas e pontos de atenção

- Um `IEnumerable<T>` devolvido por iterador guarda a receita, não os dados. Se a sequência for consumida mais de uma vez, materialize com `ToList()` para não repetir o trabalho.
- Valide argumentos em um método comum que delega para o iterador privado. Dentro do iterador, o `throw` só dispara quando alguém enumera.
- Coloque a liberação de recursos em `try/finally`. O `finally` roda no `Dispose()` do enumerador, o que cobre também o `break` no meio do `foreach`.
- Sequências infinitas com `while (true)` só são seguras enquanto o consumidor limitar o consumo (`Take`, `First`, `TakeWhile`).
- Um método iterador não pode usar `return valor`, parâmetros `ref`/`out`, nem `yield return` dentro de um bloco `try` que tenha `catch`.
- Iteradores não são thread-safe: cada thread deve obter o próprio enumerador.
- Os números de alocação da seção 4 variam entre execuções e servem como ordem de grandeza, não como benchmark.

## Conteúdo complementar

Seções executadas pelo programa:

| Seção | Arquivo | O que demonstra |
| --- | --- | --- |
| 1 | `Demos/BasicsDemo.cs` | Sintaxe mínima: sequência fixa, comparação com `List<string>`, iterador em laço e filtro simples. |
| 2 | `Demos/DeferredExecutionDemo.cs` | O corpo do iterador pausando em cada `yield return` e reexecutando do zero a cada `foreach`. |
| 3 | `Demos/ManualEnumeratorDemo.cs` | `IEnumerator<T>` escrito à mão versus a mesma sequência com `yield return`. |
| 4 | `Demos/InfiniteSequenceDemo.cs` | Fibonacci infinito consumido parcialmente e `yield break`. |
| 5 | `Demos/StreamingPipelineDemo.cs` | Pipeline `WhereLazy`/`SelectLazy`/`TakeLazy` e comparação de alocação eager versus lazy. |
| 6 | `Demos/PitfallsDemo.cs` | Múltiplas enumerações, validação adiada e `finally` no consumo parcial. |

Equivalência mental útil ao ler o código:

```csharp
// Iterador
private static IEnumerable<int> Countdown(int start)
{
    for (int value = start; value >= 1; value--)
    {
        yield return value;
    }
}

// Aproximadamente o que o compilador gera:
// uma classe com campo de estado, campo Current e MoveNext()
// que retoma a execução no ponto onde o último yield parou.
```

## Referências e documentação complementar

- [Iterators - C# guide](https://learn.microsoft.com/dotnet/csharp/iterators)
- [yield statement - C# reference](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/yield)
- [Deferred execution in LINQ](https://learn.microsoft.com/dotnet/standard/linq/deferred-execution-lazy-evaluation)
