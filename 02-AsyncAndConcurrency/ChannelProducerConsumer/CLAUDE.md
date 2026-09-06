# CLAUDE.md — ChannelProducerConsumer

Console de produtor-consumidor com `System.Threading.Channels`, focado em backpressure, canais bounded e conclusão ordenada. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/ChannelProducerConsumer/ChannelProducerConsumer.csproj
dotnet run --project 02-AsyncAndConcurrency/ChannelProducerConsumer/ChannelProducerConsumer.csproj
```

## Estrutura interna

`Channels/` tem as três peças: `ReadingProducer` (mede o tempo dentro de `WriteAsync` — é essa medida que materializa o backpressure), `ReadingConsumer` (drena com `ReadAllAsync`, sem sentinela) e `ProducerConsumerCoordinator`, que contém a lógica central: consumidores primeiro, `Task.WhenAll(produtores)`, só então `Writer.Complete()`, e por fim `Reader.Completion`.

O produtor recebe o `Channel<T>` inteiro, não só o `ChannelWriter<T>`, para poder ler `Reader.Count` e reportar a profundidade da fila — está comentado no código como licença didática.

`ProducerConsumerCoordinator.IsOrderPreservedPerSensor` verifica ordem **por sensor**, não global: cada produtor escreve numa faixa própria de sequência (`producerIndex * 1000 + i`), então não há contador compartilhado nem a corrida que ele traria.

`Demo/ChannelDemoRunner` roda 5 cenários; `Program.cs` monta o `ILoggerFactory` com `AddSimpleConsole`.

## Pontos de atenção

- TFM `net10.0` (não `net9.0` como a maioria da trilha): a máquina não tem runtime 9.0 instalado, e este sample precisa ser executado, não só compilado. Mesma decisão de [CancellationTokenPipeline](../CancellationTokenPipeline/CLAUDE.md).
- `System.Threading.Channels` faz parte do runtime; a única dependência é `Microsoft.Extensions.Logging.Console` 10.0.11, usada só para os logs.
- **Sobreposição com `JobQueueDemo`**, que também usa Channels. A divisão é real: `JobQueueDemo` usa apenas `CreateUnbounded` (fila de trabalhos, workers, `Interlocked`, menu interativo); este projeto é o único que cobre bounded, backpressure e `BoundedChannelFullMode`. Ao mexer em qualquer um dos dois, preserve essa divisão. Atenção: o `CLAUDE.md` do `JobQueueDemo` afirma cobrir "canal limitado (backpressure)" — isso não corresponde ao código dele.
- Os resultados dos cenários 4 e 5 dependem do escalonador. "Ordem embaralhada" com múltiplos consumidores é o caso esmagadoramente comum, mas não é garantido; a tabela do README já é redigida nesses termos. Não transforme isso em asserção determinística nem em teste.
- Alterar `FastProducerInterval` (20ms), `SlowConsumerTime` (150ms) ou `BoundedCapacity` (3) muda o contraste entre os cenários e invalida os números citados no README.
