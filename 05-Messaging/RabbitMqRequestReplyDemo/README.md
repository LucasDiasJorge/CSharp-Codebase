# RabbitMqRequestReplyDemo

API que implementa request/reply sobre RabbitMQ, com `CorrelationId`, fila de resposta, timeout do cliente e limpeza das requisições pendentes.

## Visão geral

Mensageria é assíncrona por natureza: a requisição sai por uma fila, a resposta volta por outra, e do ponto de vista do broker não há relação nenhuma entre as duas. Quem reconstrói o par é o `CorrelationId` — um identificador que o cliente gera, o servidor devolve intacto e o cliente usa para encontrar qual chamada estava esperando aquela resposta.

Do lado do cliente, isso vira um dicionário de requisições pendentes: cada chamada registra um `TaskCompletionSource` sob o seu `CorrelationId` e espera. Quando uma resposta chega, o consumidor procura a entrada correspondente e completa a task. É o que permite ter dezenas de chamadas em voo ao mesmo tempo sem misturar respostas.

O ponto delicado é o timeout. Ele é **do cliente**: o servidor não é avisado que alguém desistiu de esperar e provavelmente vai responder de qualquer jeito, depois. Duas consequências, e as duas precisam de tratamento explícito. A entrada pendente tem de ser removida ao expirar, senão cada timeout deixa um `TaskCompletionSource` preso no dicionário para sempre. E a resposta tardia, quando chegar, não vai encontrar dono — precisa ser descartada em silêncio, sem derrubar o consumidor.

A fila de resposta usa `amq.rabbitmq.reply-to`, a pseudo-fila do próprio RabbitMQ. Ela evita declarar uma fila por cliente — ou, pior, uma por requisição, que é o erro que transforma um RPC em milhares de filas órfãs.

## Conceitos abordados

- Request/reply sobre filas e a diferença para uma chamada de método.
- `CorrelationId` ligando resposta à requisição.
- `ReplyTo` indicando para onde responder.
- `amq.rabbitmq.reply-to` como fila de resposta sem custo de declaração.
- Dicionário de pendentes com `TaskCompletionSource`.
- Timeout do lado do cliente e o que o servidor não sabe sobre ele.
- Limpeza das pendentes como prevenção de vazamento de memória.
- Descarte de resposta órfã sem derrubar o consumidor.
- Correlação sob concorrência, com várias chamadas em voo.

## Objetivos de aprendizagem

- Implementar RPC sobre mensageria sem inventar um protocolo próprio de correlação.
- Reconhecer que o timeout do cliente não cancela o trabalho do servidor.
- Evitar o vazamento silencioso que um `finally` ausente provoca no dicionário de pendentes.
- Avaliar quando request/reply sobre broker é a ferramenta certa — e quando HTTP seria melhor.

## Estrutura do projeto

```text
RabbitMqRequestReplyDemo/
|-- Client/
|   `-- RpcClient.cs
|-- Server/
|   `-- RpcServer.cs
|-- Properties/
|   `-- launchSettings.json
|-- Program.cs
|-- RabbitMqRequestReplyDemo.csproj
|-- RabbitMqRequestReplyDemo.http
`-- README.md
```

## Como executar

Requer RabbitMQ:

```bash
docker run -d --name csharp-codebase-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Depois:

```bash
dotnet run --project 05-Messaging/RabbitMqRequestReplyDemo/RabbitMqRequestReplyDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 05-Messaging/RabbitMqRequestReplyDemo/RabbitMqRequestReplyDemo.csproj
```

A API sobe em `http://localhost:5149`. Cliente e servidor rodam no mesmo processo para o exemplo ser executável com um comando; a comunicação entre eles passa pelo broker de verdade.

Roteiro que mostra o timeout e a limpeza:

```bash
# servidor demora 3s, cliente espera 1s -> 504
curl -s -X POST http://localhost:5149/rpc/sum -H "Content-Type: application/json" \
  -d '{"a":9,"b":9,"delayMs":3000,"timeoutMs":1000}'

# logo apos: pendentes volta a zero
curl -s http://localhost:5149/pending

# alguns segundos depois: a resposta tardia chegou e foi descartada
curl -s http://localhost:5149/pending
```

## Boas práticas e pontos de atenção

- Sempre devolva o `CorrelationId` recebido, sem alterar. É o único elo entre resposta e requisição; trocá-lo ou omiti-lo transforma toda resposta em órfã.
- Remova a entrada pendente em `finally`, não apenas no caminho de sucesso. Cada timeout sem limpeza deixa um `TaskCompletionSource` retido — vazamento que só aparece como memória crescendo devagar.
- Resposta órfã é normal, não erro. Ela acontece sempre que o cliente desiste antes de o servidor responder. Descarte em silêncio; deixar a exceção subir derruba o consumidor de respostas e aí nenhuma resposta mais é entregue.
- O timeout é do cliente e não cancela nada no servidor. Se o trabalho tem efeito colateral, ele vai acontecer mesmo depois de o cliente desistir — e o cliente precisa estar preparado para isso (idempotência, reconciliação).
- Prefira `amq.rabbitmq.reply-to` a uma fila de resposta declarada. Uma fila por requisição gera custo de declaração, e qualquer falha no caminho deixa filas órfãs acumulando no broker.
- Não use `autoAck: false` na pseudo-fila `amq.rabbitmq.reply-to`: ela exige `autoAck: true`.
- Responda também aos erros. Uma requisição com payload inválido que fica sem resposta faz o cliente esperar o timeout inteiro por algo que já se sabe que não virá.
- Pense duas vezes antes de escolher request/reply sobre broker. Ele acrescenta um intermediário, latência e dois pontos de falha a uma interação que é sincrônica por natureza. Faz sentido quando já existe a infraestrutura de filas, quando o trabalho precisa de balanceamento entre consumidores ou quando o cliente não alcança o servidor diretamente; para chamada simples entre dois serviços que se enxergam, HTTP costuma ser mais simples e mais fácil de operar.

## Conteúdo complementar

Fluxo completo:

```text
cliente                                 broker                          servidor
   |                                      |                                |
   |-- publica em rpc.requests ---------->|                                |
   |   CorrelationId: abc123              |-- entrega -------------------->|
   |   ReplyTo: amq.rabbitmq.reply-to     |                                |
   |                                      |                      processa a requisicao
   |   (guarda abc123 -> TCS no           |<-- publica em reply-to --------|
   |    dicionario de pendentes)          |    CorrelationId: abc123       |
   |<-- entrega a resposta ---------------|                                |
   |                                      |                                |
   | acha abc123, completa a TCS, remove do dicionario                     |
```

Endpoints:

| Rota | Finalidade |
|---|---|
| `POST /rpc/sum` | Requisição única, com atraso e timeout configuráveis |
| `POST /rpc/concurrent` | N requisições simultâneas, conferindo a correlação |
| `GET /pending` | Pendentes em aberto e respostas tardias descartadas |

Resultados observados:

| Cenário | Atraso do servidor | Timeout do cliente | Resultado |
|---|---|---|---|
| Normal | 0ms | 3000ms | 200, respondido em ~18ms |
| Lento, dentro do prazo | 2000ms | 3000ms | 200, respondido em ~2006ms |
| Lento além do prazo | 3000ms | 1000ms | **504**, abortado em ~1002ms |

Depois do cenário de timeout:

```json
// imediatamente apos o 504
{ "pendentes": 0, "respostasTardiasDescartadas": 0 }

// alguns segundos depois, quando o servidor finalmente respondeu
{ "pendentes": 0, "respostasTardiasDescartadas": 2 }
```

`pendentes` volta a zero na hora — prova de que a limpeza aconteceu no `finally`. As respostas tardias chegam depois e são contabilizadas como descartadas, sem afetar nada.

Correlação sob concorrência, com 12 chamadas simultâneas:

```text
index 0  esperado 0   recebido 0
index 1  esperado 2   recebido 2
index 2  esperado 4   recebido 4
...
index 11 esperado 22  recebido 22

total 12, respondidas 12, expiradas 0
```

Cada resposta traz a soma do próprio índice. Com o `CorrelationId` trocado, apareceria aqui o resultado de outra requisição.

Comparação com alternativas:

| | Request/reply em fila | HTTP direto |
|---|---|---|
| Acoplamento de endereço | Cliente só conhece a fila | Cliente precisa alcançar o servidor |
| Balanceamento | Natural, por consumidores | Precisa de load balancer |
| Latência | Maior, passa pelo broker | Menor |
| Pontos de falha | Cliente, broker, servidor | Cliente, servidor |
| Timeout cancela o trabalho? | Não | Não, mas o servidor percebe a desconexão |

Relação com os vizinhos da trilha: `RabbitMQ/Send` e `RabbitMQ/Receive` cobrem o fluxo unidirecional; este projeto trata do caso em que se espera resposta. `DeadLetterQueueDemo` cuida do que fazer quando o processamento falha.

## Referências e documentação complementar

- https://www.rabbitmq.com/tutorials/tutorial-six-dotnet
- https://www.rabbitmq.com/direct-reply-to.html
- https://www.enterpriseintegrationpatterns.com/patterns/messaging/RequestReply.html
- https://www.enterpriseintegrationpatterns.com/patterns/messaging/CorrelationIdentifier.html
