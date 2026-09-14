# CLAUDE.md — ServerSentEventsDemo

Minimal API que transmite eventos por Server-Sent Events, com retomada por `Last-Event-ID` e encerramento por cancelamento. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/ServerSentEventsDemo/ServerSentEventsDemo.csproj
dotnet run --project 03-WebAPIs/ServerSentEventsDemo/ServerSentEventsDemo.csproj
```

Sobe em `http://localhost:5173`. A raiz serve a página com `EventSource`. Pela linha de comando, **use `curl -N`** — sem isso o curl bufferiza e o fluxo parece travado.

## Estrutura interna

`Events/PriceTickFeed` é ao mesmo tempo `BackgroundService` e fonte de eventos, registrado duas vezes em `Program.cs` (`AddSingleton` + `AddHostedService(sp => sp.GetRequiredService<...>())`) para que exista **uma única instância**. O gerador central é o que dá sentido ao `Last-Event-ID`: com um gerador por conexão, o id não significaria a mesma coisa para dois clientes.

O ponto delicado de `SubscribeAsync` é a ordem: captura-se `_tickSignal.Task` **antes** de ler o histórico. Na ordem inversa, um tick publicado entre a leitura e a espera seria perdido. `Publish()` completa o sinal anterior e coloca outro no lugar, tudo sob o mesmo lock.

`Endpoints/NativeSseEndpoints` usa `TypedResults.ServerSentEvents`; `Endpoints/ManualSseEndpoints` escreve o mesmo fluxo à mão, e existe para expor o formato. `Endpoints/LastEventIdReader` lê o header ou a query string equivalente.

## Pontos de atenção

- TFM `net10.0` — aqui não é só preferência: `TypedResults.ServerSentEvents` e `System.Net.ServerSentEvents.SseItem<T>` **só existem a partir do .NET 10**. Não rebaixar o TFM.
- Sem pacote externo. Template `web` (Minimal API), não `webapi`.
- **Armadilha verificada na prática:** `TypedResults.ServerSentEvents(items, eventType: "price")` com `items` do tipo `IAsyncEnumerable<SseItem<T>>` **compila e responde 200, mas entrega lixo** — casa com a sobrecarga genérica `ServerSentEvents<T>(IAsyncEnumerable<T>, string)`, que serializa o `SseItem` inteiro (`data`, `eventType`, `eventId`, `reconnectionInterval`) dentro do campo `data:` e não emite `id:` nem `retry:`. A sobrecarga de `SseItem<T>` **não tem** parâmetro `eventType`, porque cada item carrega o próprio. Se alguém "consertar" adicionando o argumento, o fluxo quebra silenciosamente. Confira sempre com `curl -N`, não pelo compilador.
- No formato do endpoint nativo, `id:` sai **depois** de `data:`. É válido pelo protocolo (a ordem dos campos é livre; só a linha em branco delimita), mas difere do endpoint manual, que emite `id:` primeiro. Não "uniformize" um pelo outro — a diferença é do framework, não um descuido.
- O histórico é limitado a 50 eventos. Uma retomada muito atrasada perde eventos de propósito: é o que um servidor real faz.
- `Random` tem seed fixa (`20260913`), então a sequência de símbolos e preços se repete entre execuções. Facilita comparar saídas; não confunda com determinismo de ids, que dependem do tempo de vida do processo.
- `/stream/prices` e `/stream/raw` **não terminam sozinhos**. Ao testar, sempre limite o cliente (`--max-time`), ou o comando fica pendurado.
- **Fronteira com os vizinhos**: `AsyncStreamsDemo` (trilha 02) cobre `IAsyncEnumerable` e `[EnumeratorCancellation]` na linguagem; aqui o mesmo recurso vira transporte HTTP. Não duplicar o conteúdo de lá.
