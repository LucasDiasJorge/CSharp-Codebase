# AsyncLockingDemo

Projeto console que reproduz uma race condition em uma seção crítica assíncrona e a corrige com `SemaphoreSlim`, medindo o custo de cada alternativa sobre a thread pool.

## Visão geral

O exemplo simula depósitos concorrentes em uma conta. Cada depósito faz um read-modify-write partido em dois: lê o saldo, espera 20ms de I/O simulado e escreve o valor de volta. O `await` no meio da sequência devolve a thread — e é exatamente essa janela que permite a várias operações lerem o mesmo saldo e sobrescreverem umas às outras.

São quatro cenários sobre o mesmo experimento, trocando apenas a estratégia de sincronização: sem proteção, com `lock` (que obriga a bloquear a thread, já que `await` dentro de `lock` é erro de compilação), com `SemaphoreSlim.Wait()` síncrono e com `SemaphoreSlim.WaitAsync()`. Os três últimos chegam ao saldo correto; o que os separa é quanto custa esperar.

Cada cenário é instrumentado com duas medidas: quantas threads a pool precisou injetar e qual o maior intervalo entre batidas de um heartbeat de 25ms agendado na própria pool. É esse segundo número que expõe o problema — quando a espera bloqueia threads, o heartbeat simplesmente para de bater.

Depois dos cenários, o exemplo demonstra as duas armadilhas que aparecem assim que `SemaphoreSlim` entra no código: esquecer o `Release` e supor que ele é reentrante como o `lock`.

## Conceitos abordados

- Race condition em read-modify-write que atravessa um `await`.
- Por que `await` não é permitido dentro de `lock` (CS1996) e por que `Monitor` não serve para seção crítica assíncrona.
- `SemaphoreSlim(1, 1)` como mutex assíncrono e `WaitAsync` como espera que não ocupa thread.
- Sync-over-async (`.GetAwaiter().GetResult()`) e thread pool starvation.
- Injeção de threads da pool e seu efeito sobre a latência de tudo o mais que roda no processo.
- `Release` em `finally` como condição para o semáforo sobreviver a exceções.
- Não reentrância do `SemaphoreSlim` em contraste com a reentrância do `Monitor`.
- `WaitAsync` com timeout como forma de transformar deadlock em diagnóstico.

## Objetivos de aprendizagem

- Reconhecer que o trecho entre a leitura e a escrita, e não a linha individual, é a seção crítica.
- Entender por que trocar `lock` por `SemaphoreSlim` só resolve se a espera também deixar de ser bloqueante.
- Ler starvation da thread pool por sintomas indiretos, como latência de tarefas não relacionadas.
- Escrever uma seção crítica assíncrona correta: `WaitAsync`, `try`, `finally`, `Release`.
- Antecipar os dois modos de falha do `SemaphoreSlim`: vaga nunca devolvida e aquisição aninhada.

## Estrutura do projeto

```text
AsyncLockingDemo/
|-- Accounts/
|   |-- BlockingLockAccount.cs
|   |-- BlockingSemaphoreAccount.cs
|   |-- IAsyncAccount.cs
|   |-- SemaphoreAccount.cs
|   `-- UnsafeAccount.cs
|-- Demo/
|   |-- AsyncLockingDemoRunner.cs
|   |-- ContentionRunner.cs
|   |-- ReleasePitfallDemo.cs
|   `-- ThreadPoolProbe.cs
|-- Models/
|   |-- ScenarioResult.cs
|   `-- ThreadPoolSnapshot.cs
|-- AsyncLockingDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/AsyncLockingDemo/AsyncLockingDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 02-AsyncAndConcurrency/AsyncLockingDemo/AsyncLockingDemo.csproj
```

A execução leva cerca de 15 segundos e não exige serviço externo. A maior parte desse tempo está no cenário 2: a lentidão é o resultado que ele produz, não um defeito do exemplo.

## Boas práticas e pontos de atenção

- A seção crítica é o intervalo entre a leitura e a escrita, não cada instrução isolada. Um `Interlocked.Increment` não resolveria este caso, porque o valor lido precisa sobreviver a um `await`.
- `await` dentro de `lock` é erro de compilação, e isso é proteção, não limitação: `Monitor` é preso à thread que adquiriu o lock, e a continuação de um `await` pode voltar em outra thread.
- Use `SemaphoreSlim(1, 1)` quando precisar de exclusão mútua com `await` no meio. Com contagem maior que 1, o mesmo tipo vira limitador de concorrência — outro objetivo, coberto em `ParallelDataProcessingDemo` e `InvoiceThrottlingApi`.
- `Release` sempre em `finally`. Sem ele, uma exceção na seção crítica deixa o semáforo permanentemente fechado, e o sintoma em produção é travamento silencioso, não exceção.
- `SemaphoreSlim` não é reentrante. Chamar um método protegido de dentro de outro método protegido pelo mesmo semáforo trava para sempre; `lock`, por ser reentrante, esconderia o problema.
- Não chame `Release()` mais vezes do que `Wait`/`WaitAsync`: o excesso lança `SemaphoreFullException` quando o construtor recebeu `maxCount`.
- Use `WaitAsync` com timeout quando a espera não puder ser infinita. É a diferença entre um deadlock e um log que diz o que aconteceu.
- Um semáforo protege apenas o processo atual. Coordenação entre instâncias exige lock distribuído, não `SemaphoreSlim`.
- O amostrador de threads em `ThreadPoolProbe` roda em uma `Thread` dedicada, e não em uma `Task`, justamente porque precisa continuar medindo quando a pool está saturada.

## Conteúdo complementar

Saída típica com 24 depósitos concorrentes, I/O simulado de 20ms e 8 processadores lógicos:

| Estratégia | Saldo final | Tempo | Threads da pool | Maior intervalo do heartbeat |
|---|---|---|---|---|
| Sem sincronização | 1 de 24 | ~24ms | 8 | ~0ms |
| `lock` + sync-over-async | 24 | ~14.000ms | 8 -> 25 | ~13.600ms |
| `SemaphoreSlim.Wait()` | 24 | ~1.500ms | sem injeção nova | ~870ms |
| `SemaphoreSlim.WaitAsync()` | 24 | ~730ms | sem injeção nova | ~40ms |

Como ler a tabela:

- A primeira linha erra o saldo porque todas as operações leem o mesmo valor inicial e a última escrita vence. O número exato varia entre execuções; a direção, não.
- A segunda linha acerta o saldo, mas paga caro: as tasks em espera ficam presas em threads, a pool injeta novas threads a cerca de uma por segundo e o heartbeat fica quase 14 segundos sem rodar. Em uma aplicação web, esse heartbeat seriam as outras requisições.
- A terceira linha existe para isolar a variável: trocar `lock` por `SemaphoreSlim` mantendo a espera bloqueante não conserta a starvation. O que muda é apenas a ferramenta, não o comportamento.
- A quarta linha resolve: `WaitAsync` libera a thread enquanto a vaga não abre, então não há injeção nem atraso perceptível no heartbeat.

Atenção ao comparar os tempos das linhas 2 e 3: a thread pool não encolhe entre cenários, então o terceiro já começa com as threads que o segundo forçou a pool a criar. A comparação honesta entre eles é pela coluna do heartbeat, não pelo tempo total.

Os números variam conforme a máquina, o número de processadores e o escalonador. O que se mantém é a relação: espera bloqueante custa threads, espera assíncrona não.

Relação com os vizinhos da trilha: `Threads` mostra race condition e `lock` no mundo síncrono, com threads dedicadas; este projeto trata do caso em que a seção crítica atravessa um `await`, onde `lock` deixa de ser aplicável. `AtomicOperationsDemo` cobre `Interlocked`, que resolve operações atômicas indivisíveis — um problema diferente do que aparece aqui.

## Referências e documentação complementar

- https://learn.microsoft.com/dotnet/api/system.threading.semaphoreslim
- https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock
- https://learn.microsoft.com/dotnet/csharp/misc/cs1996
- https://learn.microsoft.com/dotnet/api/system.threading.threadpool
