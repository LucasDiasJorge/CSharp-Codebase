# CLAUDE.md — RabbitMqRequestReplyDemo

Request/reply sobre RabbitMQ com `CorrelationId`, timeout do cliente e limpeza das pendentes. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 05-Messaging/RabbitMqRequestReplyDemo/RabbitMqRequestReplyDemo.csproj
dotnet run --project 05-Messaging/RabbitMqRequestReplyDemo/RabbitMqRequestReplyDemo.csproj
```

**Exige RabbitMQ** em `localhost:5672`:

```bash
docker run -d --name csharp-codebase-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Sobe em `http://localhost:5149`. Roteiro em `RabbitMqRequestReplyDemo.http`.

## Estrutura interna

Cliente e servidor no **mesmo processo** de propósito, para o sample rodar com um comando. A comunicação passa pelo broker de verdade — não há atalho em memória.

`Client/RpcClient` guarda o dicionário `_pending` de `CorrelationId` → `TaskCompletionSource`. **Os dois pontos que não podem mudar:**

1. `_pending.TryRemove` está no **`finally`** de `CallAsync`. Movê-lo para o caminho de sucesso faz cada timeout reter um `TaskCompletionSource` para sempre — vazamento silencioso, sem erro, visível só como memória crescendo.
2. `HandleReplyAsync` **descarta resposta órfã em silêncio** (incrementando `_lateResponses`). Resposta sem dono é normal, não excepcional: acontece toda vez que o cliente desiste antes de o servidor responder. Lançar aqui derrubaria o consumidor e nenhuma resposta seria entregue depois.

A fila de resposta é `amq.rabbitmq.reply-to`, a pseudo-fila do RabbitMQ — **exige `autoAck: true`**; com `false` o consumo falha.

`Server/RpcServer` devolve o `CorrelationId` recebido, sem alterar, e responde até a payload inválida (deixar sem resposta faria o cliente esperar o timeout inteiro à toa).

## Pontos de atenção

- TFM `net10.0`. Pacote: `RabbitMQ.Client` 7.2.2 (API assíncrona da v7).
- **Serviço externo obrigatório:** RabbitMQ. Sem ele o `RpcServer` falha ao abrir o canal no start.
- **O timeout é do cliente e não cancela o servidor.** É a lição central e está dita no código, no README e na resposta 504. Não "melhorar" isso propagando cancelamento — o exemplo perderia o ponto, e no RabbitMQ não há mecanismo de cancelar trabalho já entregue.
- `GET /pending` é a prova do comportamento correto: `pendentes` deve ser 0 sempre que não houver requisição em voo, mesmo depois de vários timeouts. Se passar a crescer, a limpeza quebrou.
- `respostasTardiasDescartadas` é cumulativo desde o start do processo. Os números do README foram obtidos em sequência; ao repetir os testes, o contador continua de onde parou.
- Os endpoints **bloqueiam** pelo tempo do atraso configurado. Ao testar via curl, `--max-time` precisa ser maior que `delayMs`.
- `RpcResult` usa fábricas `FromResponse`/`FromTimeout` porque `Answered` já é nome de propriedade — não renomear de volta para `Answered(...)`, que não compila.
- **Fronteira com os vizinhos**: `RabbitMQ/Send` e `RabbitMQ/Receive` cobrem o fluxo unidirecional; [DeadLetterQueueDemo](../DeadLetterQueueDemo/CLAUDE.md) cobre falha de processamento. Aqui o assunto é só a correlação pedido/resposta.
