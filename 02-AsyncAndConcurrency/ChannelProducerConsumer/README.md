# ChannelProducerConsumer

Projeto console que implementa produtores e consumidores com `System.Threading.Channels`, mostrando backpressure, canais bounded e conclusao ordenada do processamento.

## Visao geral

O exemplo simula sensores de telemetria escrevendo leituras em um canal enquanto workers as consomem. Os tempos sao propositalmente desequilibrados — o produtor escreve a cada 20ms e o consumidor gasta 150ms por item — para que a pressao apareça em numeros, e nao apenas na teoria.

Cada cenario troca uma unica variavel e mede o efeito: tempo que o produtor passou bloqueado dentro de `WriteAsync`, profundidade maxima da fila, quantos itens foram descartados e se a ordem de cada sensor sobreviveu ate a saida. Ao executar, compare os cenarios 1 e 2: o mesmo trabalho leva praticamente o mesmo tempo total, mas um deles segura o produtor e o outro deixa a fila crescer.

A licao central e que um canal bounded nao serve para ser rapido, e sim para tornar a lentidao visivel e controlada: sem limite, o produtor descolado do consumidor apenas transfere o problema para a memoria.

## Conceitos abordados

- Canais bounded e unbounded com `Channel.CreateBounded` e `Channel.CreateUnbounded`.
- Backpressure via `ChannelWriter<T>.WriteAsync`, que so retorna quando ha vaga.
- Politicas de canal cheio com `BoundedChannelFullMode` (`Wait` e `DropOldest`).
- Consumo com `ChannelReader<T>.ReadAllAsync` e encerramento sem sentinela.
- Conclusao ordenada: `Writer.Complete()` depois de todos os produtores, e `Reader.Completion`.
- Efeito do numero de consumidores sobre a ordem de saida.
- Inspecao da fila com `ChannelReader<T>.CanCount` e `Count`.

## Objetivos de aprendizagem

- Escolher entre canal bounded e unbounded a partir do que acontece quando o consumidor nao acompanha.
- Reconhecer backpressure medindo o tempo gasto pelo produtor dentro de `WriteAsync`.
- Entender que `DropOldest` e `DropWrite` trocam espera por perda silenciosa de dados.
- Encerrar um pipeline sem mensagem sentinela nem flag compartilhada.
- Saber por que o canal entrega em FIFO mas a saida do processamento pode sair fora de ordem.

## Estrutura do projeto

```text
ChannelProducerConsumer/
|-- Channels/
|   |-- ProducerConsumerCoordinator.cs
|   |-- ProducerReport.cs
|   |-- ReadingConsumer.cs
|   `-- ReadingProducer.cs
|-- Demo/
|   `-- ChannelDemoRunner.cs
|-- Models/
|   |-- ChannelRunSummary.cs
|   `-- TelemetryReading.cs
|-- ChannelProducerConsumer.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/ChannelProducerConsumer/ChannelProducerConsumer.csproj
```

Para validar compilacao:

```bash
dotnet build 02-AsyncAndConcurrency/ChannelProducerConsumer/ChannelProducerConsumer.csproj
```

A execucao completa leva cerca de 6 segundos e nao exige servico externo.

## Boas praticas e pontos de atencao

- Complete o writer somente depois que **todos** os produtores terminarem. Chamar `Complete()` cedo faz as escritas seguintes lancarem `ChannelClosedException`; por isso o coordenador aguarda `Task.WhenAll(produtores)` antes de fechar.
- Inicie os consumidores antes dos produtores. `ReadAllAsync` espera sem custo enquanto o canal esta vazio.
- Prefira bounded como padrao. Um canal unbounded nunca aplica backpressure: sob carga sustentada a fila cresce ate a memoria acabar.
- `DropOldest` e `DropWrite` fazem `WriteAsync` retornar sucesso mesmo quando o item e descartado. Se a perda importa, o canal nao vai avisar — compare produzidos e consumidos, como faz o `ChannelRunSummary`.
- `SingleReader` e `SingleWriter` sao promessas suas ao runtime, nao configuracoes de comportamento. Declarar `SingleReader = true` e usar dois consumidores corrompe o estado do canal.
- Um so consumidor preserva a ordem; varios consumidores nao. Se a ordem por chave importa, particione as chaves entre canais em vez de aumentar o numero de consumidores.
- `ChannelReader<T>.Count` exige checar `CanCount` antes; nem toda implementacao sabe se contar.
- O provider de console do `Microsoft.Extensions.Logging` grava em fila propria; por isso o runner aguarda um instante antes de imprimir o resumo de cada cenario, para que a ordem na tela reflita a ordem real.

## Conteudo complementar

Cenarios executados pelo `ChannelDemoRunner`, todos com produtor de 20ms e consumidor de 150ms salvo indicacao:

| # | Cenario | Variavel trocada | O que observar |
|---|---------|------------------|----------------|
| 1 | Bounded capacidade 3, `Wait` | politica padrao | Produtor bloqueia por centenas de ms; fila nunca passa de 3 |
| 2 | Unbounded | sem limite | Produtor nao espera; fila chega perto do total produzido |
| 3 | Bounded capacidade 3, `DropOldest` | politica de descarte | Produtor livre, mas parte das leituras nunca chega ao consumidor |
| 4 | 3 produtores / 2 consumidores | concorrencia de escrita | 18 produzidos e 18 consumidos: `Complete()` no momento certo nao perde nada |
| 5 | 1 consumidor versus 3 consumidores | concorrencia de leitura | Com 1, a ordem por sensor se mantem; com 3, deixa de valer |

Os numeros exatos variam entre execucoes por dependerem do escalonador. O que se mantem e a direcao: bounded troca memoria por espera, unbounded troca espera por memoria, e descarte troca ambos por perda de dados.

Politicas disponiveis em `BoundedChannelFullMode`:

```text
Wait        -> WriteAsync espera abrir vaga (padrao; aplica backpressure)
DropOldest  -> descarta o item mais antigo da fila e aceita o novo
DropNewest  -> descarta o item mais recente da fila e aceita o novo
DropWrite   -> descarta o item que estava sendo escrito
```

Relacao com os vizinhos da trilha: `JobQueueDemo` cobre fila de trabalhos com canal unbounded, multiplos workers e estatisticas; este projeto foca no que aquele nao trata — limite de capacidade, backpressure e politicas de canal cheio.

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/core/extensions/channels
- https://learn.microsoft.com/dotnet/api/system.threading.channels.channel
- https://learn.microsoft.com/dotnet/api/system.threading.channels.boundedchannelfullmode
