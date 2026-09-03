# SpanAndMemoryDemo

Projeto console que compara arrays, `Span<T>` e `Memory<T>` em operacoes de slicing e parsing.

## Visao geral

O exemplo processa uma linha simples no formato `MSFT,25,412.35`. Ele mostra que slicing com array cria copias, enquanto `ReadOnlySpan<char>` permite apontar para partes do mesmo buffer durante parsing sincronico.

O projeto tambem usa `ReadOnlyMemory<char>` para atravessar uma fronteira assincrona e explica por que `Memory<T>` e mais adequado que `Span<T>` quando o dado precisa viver alem do escopo imediato.

## Conceitos abordados

- Slicing de arrays com copia.
- `ReadOnlySpan<char>` para parsing sem slices intermediarios alocados.
- `ReadOnlyMemory<char>` em fluxo assincrono.
- Tempo de vida e restricoes de `Span<T>`.
- Medicao aproximada de alocacoes com `GC.GetAllocatedBytesForCurrentThread`.

## Objetivos de aprendizagem

- Comparar o custo de slicing em arrays com slices baseados em span.
- Entender por que `Span<T>` e seguro para buffers temporarios e escopos curtos.
- Usar `Memory<T>` quando o buffer precisa ser guardado ou atravessar `await`.
- Identificar onde ainda ha alocacao, como na criacao da string final de saida.

## Estrutura do projeto

```text
SpanAndMemoryDemo/
|-- Demo/
|   `-- SpanMemoryDemoRunner.cs
|-- Models/
|   |-- SliceReport.cs
|   `-- TradeOrder.cs
|-- Services/
|   |-- AllocationMeter.cs
|   |-- ArraySlicingDemo.cs
|   |-- MemoryProcessingDemo.cs
|   `-- SpanParsingDemo.cs
|-- Program.cs
|-- README.md
`-- SpanAndMemoryDemo.csproj
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/SpanAndMemoryDemo/SpanAndMemoryDemo.csproj
```

Para validar compilacao:

```bash
dotnet build 01-Fundamentals/SpanAndMemoryDemo/SpanAndMemoryDemo.csproj
```

## Boas praticas e pontos de atencao

- Use `Span<T>` e `ReadOnlySpan<T>` para processamento sincronico de buffers temporarios.
- Use `Memory<T>` e `ReadOnlyMemory<T>` quando o buffer precisa ser armazenado, passado para outro objeto ou usado em APIs async.
- Evite converter slices para `string` cedo demais; cada conversao materializa uma nova alocacao.
- Prefira APIs de parsing que aceitam span quando o objetivo e reduzir alocacoes intermediarias.

## Conteudo complementar

Resumo rapido:

```text
Array slice       -> copia dados para outro array
Span<T>           -> view temporaria, stack-only/ref struct
ReadOnlySpan<T>   -> view temporaria de leitura
Memory<T>         -> view armazenavel e compativel com async
ReadOnlyMemory<T> -> view armazenavel de leitura
```

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/standard/memory-and-spans/
- https://learn.microsoft.com/dotnet/api/system.span-1
- https://learn.microsoft.com/dotnet/api/system.memory-1
