# CLAUDE.md — ParallelDataProcessingDemo

Console que compara sequencial, `Task.WhenAll` e `Parallel.ForEachAsync` sobre cargas de I/O e de CPU, medindo tempo e concorrência real. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/ParallelDataProcessingDemo/ParallelDataProcessingDemo.csproj
dotnet run --project 02-AsyncAndConcurrency/ParallelDataProcessingDemo/ParallelDataProcessingDemo.csproj
```

## Estrutura interna

Duas dimensões que se cruzam: `Strategies/` (como percorrer o lote) × `Workloads/` (o que fazer com cada item). O runner mede o produto cartesiano relevante.

- `Strategies/` — `SequentialStrategy`, `WhenAllStrategy`, `ThrottledWhenAllStrategy` (`SemaphoreSlim`) e `ParallelForEachAsyncStrategy` (`MaxDegreeOfParallelism`), todas atrás de `IProcessingStrategy`.
- `Workloads/` — `IoBoundWorkload` (`Task.Delay`) e `CpuBoundWorkload`, este com o flag `offloadToThreadPool` que liga/desliga o `Task.Run`.
- `Workloads/ConcurrencyTracker` — mede o **pico real** de execuções simultâneas com `Interlocked.Increment` e um laço `CompareExchange` para o máximo. É o instrumento que prova o comportamento; sem ele o sample seria só cronometragem.
- `Demo/ParallelDemoRunner` roda 4 cenários e imprime tabela com ganho relativo ao sequencial e checksum de conferência.

## Pontos de atenção

- TFM `net10.0` (não `net9.0` como a maioria da trilha): a máquina não tem runtime 9.0 instalado, e este sample precisa ser executado, não só compilado. Mesma decisão de [CancellationTokenPipeline](../CancellationTokenPipeline/CLAUDE.md) e [ChannelProducerConsumer](../ChannelProducerConsumer/CLAUDE.md).
- Única dependência: `Microsoft.Extensions.Logging.Console` 10.0.11.
- **O cenário 3 é o coração do sample.** `CpuBoundWorkload` com `offloadToThreadPool: false` retorna `ValueTask.CompletedTask` de propósito: é isso que faz `Task.WhenAll` degenerar em sequencial (pico 1, ganho 0.88x) enquanto `Parallel.ForEachAsync` mantém 4.51x. Não "conserte" esse caminho adicionando `await` ou `Task.Run` — ele existe para falhar.
- `IterationsPerItem` (12M) foi calibrado para ~75ms por item **em Debug**. Em Release o laço fica bem mais rápido e os tempos absolutos do README deixam de bater; as proporções entre estratégias se mantêm.
- Os números das tabelas do README vieram de uma execução real em máquina de 8 processadores lógicos. Alterar `IoItemCount` (12), `CpuItemCount` (8), `IoLatency` (100ms) ou `IterationsPerItem` invalida essas tabelas.
- Sobreposição deliberada com `TaskWhenAll/example` e `AsyncTasksDemo`, que mostram `Task.WhenAll` com 2-3 tasks fixas. A divisão: aqueles são introdutórios, este trata de coleção e grau de paralelismo. Mantenha a separação.
