# DeadLetterQueueDemo

API que consome de uma fila RabbitMQ com retentativa limitada, atraso entre tentativas, envio para dead-letter queue e reprocessamento controlado.

## Visão geral

O erro clássico ao consumir fila é rejeitar a mensagem com `requeue: true`. Ela volta para a frente da fila, é entregue de novo, falha de novo, e o ciclo não termina nunca. Uma única mensagem defeituosa — a *poison message* — passa a ocupar o consumidor em tempo integral e tudo o que estava atrás dela para de ser processado.

A saída tem três partes. Primeiro, um limite de tentativas: depois de N falhas, a mensagem sai do caminho principal. Segundo, um atraso entre tentativas, para não repetir a falha mil vezes por segundo enquanto a dependência não volta. Terceiro, um destino final onde a mensagem fica registrada com o motivo, em vez de ser descartada.

O atraso vem de um truque de topologia que não exige plugin: a fila de espera não tem consumidor nenhum. A mensagem fica lá até o TTL expirar, e a própria expiração a devolve para a exchange principal, porque a dead-letter exchange *da fila de espera* aponta de volta para lá. Esperar sem ocupar consumidor.

A distinção que mais economiza tempo é entre falha transitória e permanente. Banco fora do ar vale retentar; JSON malformado não vai passar a ser válido na terceira tentativa. Aqui, falha permanente vai direto para a DLQ com uma única tentativa.

## Conceitos abordados

- Poison message e o ciclo infinito do `requeue: true`.
- Limite de tentativas com contagem propagada por cabeçalho.
- Atraso entre tentativas via fila de espera com TTL e `x-dead-letter-exchange`.
- Dead-letter exchange e dead-letter queue.
- Falha transitória versus permanente, e por que a segunda não deve ser retentada.
- Motivo do descarte gravado no cabeçalho da mensagem morta.
- Reprocessamento controlado a partir da DLQ.
- Inspeção da DLQ sem consumi-la.
- `BasicQos` e o efeito do prefetch sobre o controle de retentativa.

## Objetivos de aprendizagem

- Reconhecer por que `requeue: true` é uma armadilha e o que colocar no lugar.
- Montar retentativa com atraso no RabbitMQ sem plugin, só com topologia.
- Classificar falhas antes de decidir retentar.
- Tratar a DLQ como estacionamento com saída, não como cemitério.
- Entender por que a DLQ precisa registrar o motivo junto da mensagem.

## Estrutura do projeto

```text
DeadLetterQueueDemo/
|-- Messaging/
|   |-- QueueTopology.cs
|   `-- RabbitMqConnection.cs
|-- Processing/
|   |-- OrderConsumer.cs
|   |-- OrderProcessor.cs
|   `-- ProcessingLog.cs
|-- Properties/
|   `-- launchSettings.json
|-- DeadLetterQueueDemo.csproj
|-- DeadLetterQueueDemo.http
|-- Program.cs
`-- README.md
```

## Como executar

Requer RabbitMQ:

```bash
docker run -d --name csharp-codebase-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Depois:

```bash
dotnet run --project 05-Messaging/DeadLetterQueueDemo/DeadLetterQueueDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 05-Messaging/DeadLetterQueueDemo/DeadLetterQueueDemo.csproj
```

A API sobe em `http://localhost:5054` e declara a topologia sozinha no start. O painel do RabbitMQ fica em `http://localhost:15672` (guest/guest).

Roteiro completo:

```bash
curl -s -X POST http://localhost:5054/orders -H "Content-Type: application/json" \
  -d '{"orderId":"OK-1","behavior":"ok"}'
curl -s -X POST http://localhost:5054/orders -H "Content-Type: application/json" \
  -d '{"orderId":"TR-1","behavior":"transient","succeedOnAttempt":2}'
curl -s -X POST http://localhost:5054/orders -H "Content-Type: application/json" \
  -d '{"orderId":"PO-1","behavior":"poison"}'
curl -s -X POST http://localhost:5054/orders -H "Content-Type: application/json" \
  -d '{"orderId":"IN-1","behavior":"invalid"}'

sleep 16
curl -s http://localhost:5054/attempts
curl -s http://localhost:5054/queues
```

## Boas práticas e pontos de atenção

- Nunca responda a uma falha com `BasicNack(requeue: true)` sem limite. A mensagem volta na hora, falha na hora e monopoliza o consumidor. É a forma mais rápida de derrubar o processamento de uma fila inteira com uma mensagem só.
- Conte as tentativas você mesmo, em um cabeçalho que viaja com a mensagem. O `x-death` que o RabbitMQ preenche sozinho é informativo e tem formato incômodo de interpretar; um contador próprio é explícito.
- Separe falha transitória de permanente antes de decidir. Retentar um payload que não desserializa gasta três ciclos e alguns segundos para chegar à mesma conclusão da primeira tentativa.
- Grave o motivo junto da mensagem morta. Uma DLQ com mil mensagens e nenhum `reason` obriga a reconstruir cada incidente a partir do log, quando ele ainda existir.
- A DLQ precisa de uma saída. Sem um caminho de reprocessamento, ela é só um lugar onde mensagens vão morrer em silêncio — e alguém vai acabar dando purge.
- Reprocessar é decisão deliberada, tomada depois de corrigir a causa. Uma DLQ que se reprocessa sozinha é apenas um ciclo infinito mais lento.
- Ao inspecionar a DLQ, leia tudo antes de devolver. Fazer `BasicGet` e `BasicNack(requeue: true)` dentro do mesmo laço recoloca a mensagem na fila imediatamente e o próximo `BasicGet` a pega de novo — laço infinito.
- Configure `BasicQos`. Sem limite de prefetch, o broker despeja a fila no consumidor e a ordem das tentativas fica imprevisível.
- Monitore a profundidade da DLQ. Ela deveria ser sempre zero; qualquer valor diferente é trabalho parado esperando alguém.

## Conteúdo complementar

Topologia declarada no start:

```text
                    +-- publica --> [orders] (exchange)
                                         |
                                         v
                                  [orders.main] ---- falha permanente ------+
                                         |                                  |
                              falha transitoria                             |
                                         |                                  |
                                         v                                  v
                            [orders.retry.queue]                     [orders.dlx]
                          TTL 3s, sem consumidor                           |
                                         |                                 v
                          expira e volta para [orders]             [orders.dlq]
```

A fila de espera não tem consumidor de propósito: a mensagem só sai dela quando o TTL expira, e a expiração a devolve para a exchange principal.

Endpoints:

| Rota | Finalidade |
|---|---|
| `POST /orders` | Publica um pedido; `behavior` decide o desfecho |
| `POST /orders/raw` | Publica um payload cru, para testar malformado |
| `GET /queues` | Profundidade das três filas |
| `GET /attempts` | Histórico de tentativas do consumidor |
| `GET /dlq` | Espia a DLQ sem consumir, com o motivo de cada mensagem |
| `POST /dlq/replay` | Devolve a DLQ para a fila principal |
| `DELETE /dlq` | Descarta a DLQ |

Comportamentos disponíveis no campo `behavior`:

| Valor | O que acontece |
|---|---|
| `ok` | Processa de primeira |
| `transient` | Falha até a tentativa indicada em `succeedOnAttempt`, depois passa |
| `poison` | Falha sempre; esgota o limite e vai para a DLQ |
| `invalid` | Falha permanente; vai direto para a DLQ |

Resultado observado no roteiro acima:

```text
OK-1   tentativa 1  Success             -> ack
TR-1   tentativa 1  TransientFailure    -> fila de espera (3s)
IN-1   tentativa 1  PermanentFailure    -> DLQ direto
PO-1   tentativa 1  TransientFailure    -> fila de espera
TR-1   tentativa 2  Success             -> ack, recuperada
PO-1   tentativa 2  TransientFailure    -> fila de espera
PO-1   tentativa 3  TransientFailure    -> DLQ, tentativas esgotadas

filas ao final: main=0  retry=0  dlq=2   (PO-1 e IN-1)
```

`TR-1` se recupera sozinha na segunda tentativa, que é o motivo de a retentativa existir. `IN-1` gasta uma única tentativa, porque retentar não mudaria nada. `PO-1` gasta as três e para.

Motivos gravados na DLQ:

```json
[
  { "reason": "regra de negocio violada: total negativo" },
  { "reason": "payload invalido: 'i' is an invalid start of a value..." },
  { "reason": "tentativas esgotadas: falha permanente disfarcada de transitoria" }
]
```

Parâmetros da política:

| Parâmetro | Valor | Onde |
|---|---|---|
| Máximo de tentativas | 3 | `QueueTopology.MaxAttempts` |
| Atraso entre tentativas | 3000ms | `QueueTopology.RetryDelayMs` |
| Prefetch | 1 | `BasicQosAsync` no consumidor |

Em produção, o atraso costuma crescer a cada tentativa (backoff exponencial), o que exige uma fila de espera por faixa de atraso ou o plugin de delayed message.

Relação com os vizinhos da trilha: `RabbitMQ/Send` e `RabbitMQ/Receive` cobrem publicar e consumir. Este projeto trata do que fazer quando o consumo falha. `TransactionalOutboxDemo` cuida do lado da produção — garantir que a mensagem chegue a existir.

## Referências e documentação complementar

- https://www.rabbitmq.com/dlx.html
- https://www.rabbitmq.com/ttl.html
- https://www.rabbitmq.com/consumer-prefetch.html
- https://www.enterpriseintegrationpatterns.com/patterns/messaging/DeadLetterChannel.html
