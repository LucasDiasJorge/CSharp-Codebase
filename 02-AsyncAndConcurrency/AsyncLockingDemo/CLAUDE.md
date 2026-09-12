# CLAUDE.md — AsyncLockingDemo

Console que reproduz uma race condition em seção crítica assíncrona e a corrige com `SemaphoreSlim`, medindo o custo de cada estratégia sobre a thread pool. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/AsyncLockingDemo/AsyncLockingDemo.csproj
dotnet run --project 02-AsyncAndConcurrency/AsyncLockingDemo/AsyncLockingDemo.csproj
```

## Estrutura interna

`Accounts/` tem quatro implementações de `IAsyncAccount` que diferem em uma única linha de sincronização, e é nesse contraste que está o exemplo: `UnsafeAccount` (corrida), `BlockingLockAccount` (`lock` com `.GetAwaiter().GetResult()` dentro), `BlockingSemaphoreAccount` (`Wait()` síncrono, para isolar a variável "bloqueante" da variável "lock") e `SemaphoreAccount` (`WaitAsync` + `try/finally`).

`Demo/ContentionRunner` aplica o mesmo experimento a todas: 24 `Task.Run` concorrentes, e mede. `Demo/ThreadPoolProbe` faz as duas medidas que dão sentido ao exemplo — um amostrador de `ThreadPool.ThreadCount` em `Thread` dedicada (não pode ser `Task`: precisa medir justamente quando a pool está saturada) e um heartbeat de 25ms agendado na pool, cujo maior intervalo é a assinatura da starvation.

`Demo/ReleasePitfallDemo` cobre `Release` esquecido e não reentrância, ambos com `WaitAsync(timeout)` para virar log em vez de deadlock. `Demo/AsyncLockingDemoRunner` orquestra e imprime a tabela final.

Parâmetros centralizados em `AsyncLockingDemoRunner`: `Operations` (24), `DepositAmount` (1) e `IoDelay` (20ms).

## Pontos de atenção

- TFM `net10.0` (não `net9.0` como a maioria da trilha): a máquina não tem runtime 9.0 instalado e este sample precisa ser executado, não só compilado. Mesma decisão de [ChannelProducerConsumer](../ChannelProducerConsumer/CLAUDE.md) e [CancellationTokenPipeline](../CancellationTokenPipeline/CLAUDE.md).
- Dependência única: `Microsoft.Extensions.Logging.Console` 10.0.11, só para os logs. `SemaphoreSlim` é do runtime.
- **O cenário 2 leva ~14s de propósito.** Isso é o resultado que ele mede (injeção de threads a ~1/s), não lentidão acidental. Não "otimize" com `ThreadPool.SetMinThreads`, ou a starvation desaparece e o exemplo perde a função.
- Aumentar `Operations` piora o tempo de forma quase linear no cenário bloqueante — 64 operações levavam ~50s. O valor 24 é o equilíbrio entre demonstrar e não cansar.
- A thread pool não encolhe entre cenários: os cenários 3 e 4 começam com as threads que o 2 injetou. Por isso o tempo total deles não é comparável ao do cenário 2, e a comparação válida é pela coluna do heartbeat. Isso está dito no log, no README e em comentário no runner — manter os três se o cenário mudar.
- A tabela final sai por `Console.WriteLine`, não pelo `ILogger`: o prefixo de timestamp/categoria quebraria o alinhamento das colunas. Há uma pausa de 200ms antes dela porque o provider de console do logging grava em fila própria.
- O saldo do cenário 1 costuma ser 1, mas é **não determinístico**; os números do README estão redigidos como típicos. Não transformar em asserção nem em teste.
- **Fronteira com os vizinhos**: `Threads` já cobre race condition e `lock` em contexto síncrono com threads dedicadas; `AtomicOperationsDemo` cobre `Interlocked`; `ParallelDataProcessingDemo` e `InvoiceThrottlingApi` usam `SemaphoreSlim` com contagem > 1 como throttle. Este projeto é o único sobre exclusão mútua atravessando `await`. Preservar essa divisão ao editar qualquer um deles.
