# ParallelDataProcessingDemo

Projeto console que compara processamento sequencial, `Task.WhenAll` e `Parallel.ForEachAsync`, controlando o grau de paralelismo e medindo o impacto em tarefas de I/O e de CPU.

## Visao geral

As mesmas quatro estrategias percorrem o mesmo lote sobre cargas diferentes. Cada passagem e cronometrada e, mais importante, tem o **pico de execucoes simultaneas** medido por um contador atomico — e esse numero, nao o nome da estrategia, que diz se houve paralelismo de verdade.

O exemplo existe para desfazer a ideia de que uma estrategia e "a mais rapida". A ordem do ranking muda conforme a carga: em I/O, disparar tudo de uma vez ganha de longe; em CPU, o teto e o numero de nucleos; e ha um caso em que `Task.WhenAll` fica **mais lento que o laco sequencial**.

Esse caso e o cenario 3. Quando o trabalho de CPU roda direto no corpo do metodo, sem `Task.Run`, cada chamada termina antes de devolver o controle e a task ja nasce concluida — `Task.WhenAll` nao tem o que aguardar e o lote degenera em sequencial, com o custo extra de alocar as tasks. `Parallel.ForEachAsync`, no mesmo codigo, continua paralelizando, porque e ele quem distribui as invocacoes entre varios workers.

## Conceitos abordados

- Comparacao entre `await` em laco, `Task.WhenAll` e `Parallel.ForEachAsync`.
- Controle do grau de paralelismo com `ParallelOptions.MaxDegreeOfParallelism`.
- Limitacao manual de concorrencia com `SemaphoreSlim`.
- Diferenca pratica entre carga de I/O e carga de CPU.
- Offload de trabalho sincrono com `Task.Run` e o que acontece sem ele.
- Medicao de concorrencia real com `Interlocked` e `CompareExchange`.

## Objetivos de aprendizagem

- Escolher a estrategia a partir do tipo de carga, e nao por habito.
- Entender por que marcar um metodo como `async` nao cria concorrencia sozinho.
- Reconhecer quando aumentar o grau de paralelismo deixa de trazer ganho.
- Limitar chamadas simultaneas a um servico externo sem abandonar `Task.WhenAll`.
- Comprovar que uma estrategia processou todos os itens, usando checksum em vez de confianca.

## Estrutura do projeto

```text
ParallelDataProcessingDemo/
|-- Demo/
|   `-- ParallelDemoRunner.cs
|-- Models/
|   |-- StrategyResult.cs
|   `-- WorkItem.cs
|-- Strategies/
|   |-- IProcessingStrategy.cs
|   |-- ParallelForEachAsyncStrategy.cs
|   |-- SequentialStrategy.cs
|   |-- ThrottledWhenAllStrategy.cs
|   `-- WhenAllStrategy.cs
|-- Workloads/
|   |-- ConcurrencyTracker.cs
|   |-- CpuBoundWorkload.cs
|   |-- IoBoundWorkload.cs
|   `-- IWorkload.cs
|-- ParallelDataProcessingDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/ParallelDataProcessingDemo/ParallelDataProcessingDemo.csproj
```

Para validar compilacao:

```bash
dotnet build 02-AsyncAndConcurrency/ParallelDataProcessingDemo/ParallelDataProcessingDemo.csproj
```

A execucao completa leva cerca de 8 segundos e nao exige servico externo.

## Boas praticas e pontos de atencao

- `Task.WhenAll` sem limite e adequado para poucos itens de I/O. Em lote grande ele dispara todas as chamadas de uma vez e pode derrubar o servico do outro lado; use `SemaphoreSlim` ou `Parallel.ForEachAsync` para impor um teto.
- Marcar um metodo como `async` nao o torna concorrente. Trabalho de CPU sincrono precisa de `Task.Run` para sair da thread chamadora — ou de um laco que distribua as invocacoes, como `Parallel.ForEachAsync`.
- Nao confundir `Parallel.ForEach` com `Parallel.ForEachAsync`. O primeiro e sincrono e nao sabe aguardar uma task: passar um delegate `async` para ele vira fire-and-forget silencioso, com excecoes perdidas.
- `MaxDegreeOfParallelism` e um teto, nao uma meta. O pico observado nunca passa do numero de itens do lote.
- Para I/O, o grau util e ditado pelo servico remoto e costuma ser bem maior que o numero de nucleos. Para CPU, passar do numero de nucleos so adiciona troca de contexto.
- Libere o `SemaphoreSlim` em `finally`. Sem isso, uma unica falha trava o restante do lote.
- O `Release` do semaforo e o `Enter`/`Exit` do contador usam operacoes atomicas porque varias threads os tocam ao mesmo tempo; um `count++` comum perderia incrementos.
- Os numeros abaixo vieram de uma execucao em Debug numa maquina de 8 processadores logicos. `dotnet run -c Release` muda os valores absolutos da carga de CPU; as proporcoes entre as estrategias e que importam.

## Conteudo complementar

Resultados de uma execucao de referencia (8 processadores logicos, build Debug):

**Cenario 1 — I/O, 12 itens de 100ms**

| Estrategia | Tempo | Pico | Ganho |
|---|---|---|---|
| Sequencial | 1313ms | 1 | 1.00x |
| `Task.WhenAll` sem limite | 110ms | 12 | 11.89x |
| `WhenAll` + `SemaphoreSlim(4)` | 326ms | 4 | 4.03x |
| `Parallel.ForEachAsync(4)` | 327ms | 4 | 4.01x |
| `Parallel.ForEachAsync(12)` | 109ms | 12 | 12.07x |

As duas formas de limitar a 4 chegam ao mesmo tempo: o que importa e o teto, nao o mecanismo.

**Cenario 2 — CPU com `Task.Run`, 8 itens**

| Estrategia | Tempo | Pico | Ganho |
|---|---|---|---|
| Sequencial | 604ms | 1 | 1.00x |
| `Task.WhenAll` sem limite | 140ms | 8 | 4.32x |
| `Parallel.ForEachAsync(2)` | 321ms | 2 | 1.88x |
| `Parallel.ForEachAsync(8)` | 152ms | 8 | 3.96x |

O ganho satura perto de 4x mesmo com pico 8, porque os 8 processadores logicos da maquina correspondem a 4 nucleos fisicos.

**Cenario 3 — CPU sem offload, 8 itens**

| Estrategia | Tempo | Pico | Ganho |
|---|---|---|---|
| Sequencial | 616ms | 1 | 1.00x |
| `Task.WhenAll` sem limite | 698ms | 1 | **0.88x** |
| `Parallel.ForEachAsync(8)` | 137ms | 8 | 4.51x |

Pico 1 com `Task.WhenAll` e a prova da armadilha: nao houve concorrencia nenhuma, e o tempo extra veio das tasks alocadas a toa.

**Cenario 4 — grau de paralelismo sobre I/O, 12 itens**

| DOP | Tempo | Pico | Ganho |
|---|---|---|---|
| 1 | 1320ms | 1 | 1.00x |
| 2 | 656ms | 2 | 2.01x |
| 4 | 328ms | 4 | 4.02x |
| 8 | 219ms | 8 | 6.03x |
| 16 | 109ms | 12 | 12.15x |

Com DOP 8 e 12 itens sao necessarias duas ondas (8 e depois 4), por isso o tempo fica em ~200ms e nao em ~150ms. Com DOP 16 o pico para em 12: o teto nunca passa do tamanho do lote.

Guia rapido de escolha:

```text
Poucos itens de I/O                 -> Task.WhenAll
Muitos itens de I/O, servico frágil -> Parallel.ForEachAsync com DOP, ou SemaphoreSlim
Trabalho de CPU                     -> Parallel.ForEachAsync com DOP ~ numero de nucleos
Limite compartilhado entre lacos    -> SemaphoreSlim criado fora dos lacos
```

Relacao com os vizinhos da trilha: `TaskWhenAll/example` e `AsyncTasksDemo` apresentam `Task.WhenAll` com duas ou tres tasks fixas; este projeto trata do caso de colecao, onde controlar o grau de paralelismo passa a ser necessario.

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallel.foreachasync
- https://learn.microsoft.com/dotnet/api/system.threading.tasks.paralleloptions
- https://learn.microsoft.com/dotnet/standard/parallel-programming/potential-pitfalls-in-data-and-task-parallelism
- https://learn.microsoft.com/dotnet/api/system.threading.semaphoreslim
