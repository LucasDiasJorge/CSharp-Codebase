# ServerSentEventsDemo

API que transmite eventos do servidor para o cliente por Server-Sent Events, demonstrando streaming unidirecional sobre HTTP, encerramento por cancelamento e retomada automática depois de uma queda de conexão.

## Visão geral

O exemplo publica cotações fictícias em um fluxo contínuo. Todo evento carrega um id sequencial, e é esse id que sustenta a parte mais útil do protocolo: quando a conexão cai, o navegador reconecta sozinho e envia o header `Last-Event-ID`, permitindo ao servidor continuar exatamente de onde parou.

O mesmo fluxo aparece escrito de duas formas. `/stream/prices` usa o suporte nativo do ASP.NET Core 10 (`TypedResults.ServerSentEvents`), que cuida dos headers e do formato. `/stream/raw` escreve os mesmos eventos byte a byte no corpo da resposta, para que o formato deixe de ser mágica: quatro campos de texto e uma linha em branco encerrando cada evento.

A fonte de eventos é única e compartilhada por todos os clientes — um `BackgroundService` gera os ticks e mantém um histórico curto. Isso não é detalhe de implementação: se cada conexão tivesse o próprio gerador, o id do evento não significaria a mesma coisa para dois clientes e a retomada seria uma ficção.

Há também uma página em `wwwroot/index.html` com um `EventSource` de verdade, onde dá para derrubar a conexão e ver a retomada acontecendo.

## Conceitos abordados

- Formato do protocolo SSE: campos `id`, `event`, `data`, `retry` e a linha em branco terminadora.
- `text/event-stream`, `Cache-Control: no-cache` e desativação de buffer em proxy reverso.
- `TypedResults.ServerSentEvents` e `SseItem<T>` no ASP.NET Core 10.
- `IAsyncEnumerable<T>` com `[EnumeratorCancellation]` como fonte do fluxo.
- Encerramento por `HttpContext.RequestAborted` quando o cliente desconecta.
- Reconexão automática do `EventSource` e o header `Last-Event-ID`.
- `retry:` como instrução de intervalo de reconexão.
- Histórico limitado e o que acontece quando a retomada chega tarde demais.
- Flush explícito como condição para o cliente receber em tempo real.

## Objetivos de aprendizagem

- Decidir entre SSE e WebSocket a partir da direção do tráfego.
- Implementar retomada de fluxo sem inventar protocolo próprio.
- Encerrar o trabalho do servidor quando o cliente desaparece, em vez de produzir para ninguém.
- Reconhecer que SSE não tem fim de stream e combinar um evento de término com o cliente.
- Entender por que o histórico do servidor precisa ser limitado e o que isso exige do cliente.

## Estrutura do projeto

```text
ServerSentEventsDemo/
|-- Endpoints/
|   |-- LastEventIdReader.cs
|   |-- ManualSseEndpoints.cs
|   `-- NativeSseEndpoints.cs
|-- Events/
|   |-- PriceTick.cs
|   `-- PriceTickFeed.cs
|-- Properties/
|   `-- launchSettings.json
|-- wwwroot/
|   `-- index.html
|-- Program.cs
|-- ServerSentEventsDemo.csproj
|-- ServerSentEventsDemo.http
`-- README.md
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/ServerSentEventsDemo/ServerSentEventsDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 03-WebAPIs/ServerSentEventsDemo/ServerSentEventsDemo.csproj
```

A aplicação sobe em `http://localhost:5173` e não exige serviço externo. Abra essa URL no navegador para usar a página com `EventSource`, ou consuma o fluxo pela linha de comando:

```bash
curl -N --max-time 5 http://localhost:5173/stream/prices
```

O `-N` desliga o buffer do curl. Sem ele, a saída aparece em blocos e o exemplo parece quebrado.

Para ver a retomada:

```bash
curl -N --max-time 3 -H "Last-Event-ID: 2" http://localhost:5173/stream/prices
```

## Boas práticas e pontos de atenção

- Repasse sempre o `HttpContext.RequestAborted` para a fonte de eventos. Sem isso, o servidor continua gerando e serializando eventos para uma conexão que já não existe — o endpoint `/connections` e os logs de desconexão existem para tornar esse vazamento visível.
- Faça flush a cada evento. Sem `FlushAsync`, a resposta fica no buffer e o cliente só recebe quando o buffer encher, o que derrota o propósito do streaming.
- `X-Accel-Buffering: no` evita que um nginx na frente segure o fluxo. É um dos motivos mais comuns de SSE funcionar em desenvolvimento e falhar em produção.
- SSE não tem "fim de fluxo". Quando o servidor fecha a conexão, o navegador reconecta. Se o término é intencional, combine um tipo de evento (`event: done`) e faça o cliente chamar `EventSource.close()`.
- Histórico limitado é o comportamento correto, não uma simplificação. Um cliente que volta depois de muito tempo vai perder eventos — trate essa lacuna no cliente em vez de manter histórico infinito no servidor.
- `EventSource` só faz `GET` e não envia headers customizados. Autenticação por header não funciona; cookie ou token na query string, sim. Se precisar enviar dados do cliente para o servidor, o caso não é SSE.
- Um `EventSource` aberto consome uma conexão HTTP/1.1 por origem, e o navegador limita a seis. Em HTTP/2 esse teto desaparece.
- Comentários de keep-alive (uma linha iniciada por `:`) mantêm viva uma conexão ociosa atrás de proxies que derrubam conexões inativas.

## Conteúdo complementar

Endpoints:

| Rota | O que demonstra |
|---|---|
| `GET /stream/prices` | Fluxo contínuo com o suporte nativo do ASP.NET Core 10 |
| `GET /stream/prices` com `Last-Event-ID` | Retomada a partir do último evento recebido |
| `GET /stream/raw` | O mesmo fluxo escrito à mão, formato cru visível |
| `GET /stream/finite?count=3` | Fluxo que termina, encerrado por um `event: done` |
| `GET /connections` | Conexões ativas e último id publicado |

Formato do protocolo, como sai do endpoint nativo:

```text
event: price
data: {"id":1,"symbol":"VALE3","price":29.01,"occurredAt":"2026-09-14T01:23:44+00:00"}
id: 1
retry: 2000

event: price
data: {"id":2,"symbol":"VALE3","price":22.05,"occurredAt":"2026-09-14T01:23:45+00:00"}
id: 2
retry: 2000
```

Campos do protocolo:

| Campo | Função |
|---|---|
| `data:` | Carga do evento. Várias linhas `data:` seguidas viram uma string com quebras de linha |
| `event:` | Nome do evento; no cliente, o argumento de `addEventListener`. Sem ele, cai em `onmessage` |
| `id:` | Identificador do evento; o navegador o devolve como `Last-Event-ID` ao reconectar |
| `retry:` | Milissegundos que o cliente deve esperar antes de reconectar |
| `: texto` | Comentário, ignorado pelo cliente. Serve como keep-alive |
| linha em branco | Encerra o evento. Sem ela, o cliente espera indefinidamente |

SSE comparado com as alternativas:

| | SSE | WebSocket | Long polling |
|---|---|---|---|
| Direção | Só servidor para cliente | Bidirecional | Só servidor para cliente |
| Protocolo | HTTP comum | Upgrade para `ws://` | HTTP comum |
| Reconexão | Automática, com retomada por id | Manual | Manual |
| Formato | Texto | Texto ou binário | Qualquer |
| Custo por conexão | Uma conexão HTTP aberta | Uma conexão aberta | Uma requisição por ciclo |

Escolha SSE quando o tráfego é só de saída — notificações, progresso de tarefa, cotações, tokens de um modelo de linguagem. Se o cliente também precisa enviar mensagens pelo mesmo canal, o caso é WebSocket.

Relação com os vizinhos: `AsyncStreamsDemo` (trilha 02) cobre `IAsyncEnumerable` e `[EnumeratorCancellation]` na linguagem; aqui esse mesmo recurso vira resposta HTTP. `ChannelProducerConsumer` trata de produtor-consumidor dentro do processo, não de transporte para o cliente.

## Referências e documentação complementar

- https://html.spec.whatwg.org/multipage/server-sent-events.html
- https://developer.mozilla.org/docs/Web/API/Server-sent_events/Using_server-sent_events
- https://learn.microsoft.com/dotnet/api/system.net.serversentevents
- https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses
