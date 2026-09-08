# CLAUDE.md — AsyncStreamsDemo

Console sobre `IAsyncEnumerable<T>`: iteradores assíncronos, `await foreach` e cancelamento com `[EnumeratorCancellation]`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/AsyncStreamsDemo/AsyncStreamsDemo.csproj
dotnet run --project 02-AsyncAndConcurrency/AsyncStreamsDemo/AsyncStreamsDemo.csproj
```

## Estrutura interna

- `Sources/SensorFeed` — três formas da mesma coleta: `StreamAsync` (iterador correto, com `[EnumeratorCancellation]` e `try/finally`), `CollectAllAsync` (contraponto materializado, para medir tempo até o primeiro item) e `StreamIgnoringCancellationAsync` (a versão quebrada de propósito).
- `Sources/PagedCatalogClient` — API paginada exposta como stream de itens, contando páginas realmente buscadas. É esse contador que prova a preguiça nos cenários 2 e 5.
- `AsyncOperators.cs` — `WhereAsync`/`SelectAsync`/`TakeAsync` escritos à mão como iteradores assíncronos, em vez de usar `System.Linq.Async`, para deixar a mecânica visível.
- `Demo/AsyncStreamsDemoRunner` roda 5 cenários. No cenário 3 os dois laços de consumo são idênticos letra por letra — a única diferença está em qual método da fonte é chamado, e é isso que isola a causa.

## Pontos de atenção

- **O build emite 1 aviso `CS8425`, e ele é intencional.** Aponta `SensorFeed.StreamIgnoringCancellationAsync`, cujo `CancellationToken` não tem `[EnumeratorCancellation]` — exatamente o defeito que o cenário 3 demonstra. Não adicione o atributo lá, não use `#pragma warning disable` e não trate como build sujo: o compilador avisando é parte da lição. A versão correta é `StreamAsync`, logo acima no mesmo arquivo.
- TFM `net10.0` (não `net9.0` como a maioria da trilha): a máquina não tem runtime 9.0 instalado, e este sample precisa ser executado, não só compilado. Mesma decisão de [CancellationTokenPipeline](../CancellationTokenPipeline/CLAUDE.md), [ChannelProducerConsumer](../ChannelProducerConsumer/CLAUDE.md) e [ParallelDataProcessingDemo](../ParallelDataProcessingDemo/CLAUDE.md).
- Única dependência: `Microsoft.Extensions.Logging.Console` 10.0.11.
- Não é possível colocar `yield return` dentro de um `try` com `catch`; só `try/finally` é permitido. O `finally` do `SensorFeed.StreamAsync` existe para mostrar que `break` e cancelamento disparam `DisposeAsync`.
- Sobreposição deliberada com `01-Fundamentals/YieldReturnDemo`, que cobre o iterador **síncrono** em profundidade (máquina de estados, execução adiada, armadilhas). Este projeto é a contraparte async e não repete aquele conteúdo. Ao mexer em qualquer um dos dois, preserve a divisão sync/async.
- Alterar `ReadingInterval` (150ms), `CancellationDelay` (500ms), `PageSize` (4) ou `TotalPages` (5) muda os números citados na tabela do README.
