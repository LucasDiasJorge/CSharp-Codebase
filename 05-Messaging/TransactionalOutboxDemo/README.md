# TransactionalOutboxDemo

API que grava dados e eventos na mesma transação e publica as mensagens depois, eliminando o dual write entre banco e broker.

## Visão geral

Salvar no banco e publicar no broker são duas operações em dois sistemas, cada uma com a própria chance de falhar. Se o banco commita e a publicação falha, o pedido existe e nenhum consumidor jamais fica sabendo. Se a ordem for invertida, o evento sai para um pedido que talvez não exista. Não há ordem correta: o problema é ter dois destinos e nenhuma transação que os cubra.

O outbox resolve trocando a segunda escrita remota por uma escrita local. O pedido e a mensagem vão para o **mesmo banco, na mesma transação** — ou os dois existem, ou nenhum. A publicação vira um passo separado, executado por um relay que lê as mensagens pendentes e as envia. Se o broker estiver fora, elas ficam pendentes e saem quando ele voltar.

O exemplo põe os dois caminhos lado a lado e deixa derrubar o broker por HTTP. Com o broker fora, `POST /orders/dual-write` devolve 500 com o pedido já gravado e o evento perdido para sempre, enquanto `POST /orders` devolve 200 e a mensagem espera na outbox. Quando o broker volta, o relay publica sozinho.

A contrapartida é que a entrega passa a ser **pelo menos uma vez**: o relay pode publicar e falhar antes de marcar como publicada, e aí a mensagem sai de novo. Por isso cada mensagem carrega um `MessageId` estável — cabe ao consumidor descartar duplicatas.

## Conceitos abordados

- Dual write entre banco e broker, e por que nenhuma ordem de execução o resolve.
- Outbox: entidade e mensagem no mesmo banco, na mesma transação.
- Relay em segundo plano lendo pendentes e publicando.
- Entrega pelo menos uma vez e o `MessageId` como base da idempotência do consumidor.
- Preservação da ordem de gravação na publicação.
- Parada no primeiro erro para não quebrar ordem nem martelar broker em falha.
- Contagem de tentativas e último erro como instrumento de diagnóstico.
- Índice sobre a consulta que o relay faz a cada ciclo.

## Objetivos de aprendizagem

- Reconhecer o dual write em um código que parece correto.
- Implementar o outbox sem reintroduzir o problema por dentro do próprio banco.
- Entender por que o outbox troca perda de mensagem por duplicata, e o que isso exige do consumidor.
- Avaliar o custo do padrão: latência de publicação e uma tabela que precisa de manutenção.

## Estrutura do projeto

```text
TransactionalOutboxDemo/
|-- Domain/
|   `-- Order.cs
|-- Messaging/
|   `-- RabbitMqPublisher.cs
|-- Outbox/
|   |-- OrdersDbContext.cs
|   |-- OutboxMessage.cs
|   `-- OutboxRelay.cs
|-- Properties/
|   `-- launchSettings.json
|-- Program.cs
|-- README.md
|-- TransactionalOutboxDemo.csproj
`-- TransactionalOutboxDemo.http
```

## Como executar

Requer RabbitMQ:

```bash
docker run -d --name csharp-codebase-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Depois:

```bash
dotnet run --project 05-Messaging/TransactionalOutboxDemo/TransactionalOutboxDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 05-Messaging/TransactionalOutboxDemo/TransactionalOutboxDemo.csproj
```

A API sobe em `http://localhost:5260`. O banco SQLite é criado no primeiro start e não é versionado.

Roteiro que mostra a diferença:

```bash
# derruba o broker
curl -s -X POST http://localhost:5260/simulation/broker \
  -H "Content-Type: application/json" -d '{"down":true}'

# dual write: 500, e o evento se perde
curl -s -X POST http://localhost:5260/orders/dual-write \
  -H "Content-Type: application/json" -d '{"customer":"bruno","total":200}'

# outbox: 200, e a mensagem fica pendente
curl -s -X POST http://localhost:5260/orders \
  -H "Content-Type: application/json" -d '{"customer":"diego","total":400}'

curl -s http://localhost:5260/outbox

# religa: em ate 2 segundos a pendente e publicada sozinha
curl -s -X POST http://localhost:5260/simulation/broker \
  -H "Content-Type: application/json" -d '{"down":false}'
```

## Boas práticas e pontos de atenção

- A outbox precisa estar no **mesmo banco** da entidade. Em outro banco, gravar nos dois volta a ser dual write — apenas com nomes diferentes.
- Não basta chamar `SaveChanges` duas vezes. Sem transação explícita, são dois commits, e uma falha entre eles deixa o pedido sem a mensagem. O endpoint `POST /orders` abre a transação justamente por isso.
- O relay entrega pelo menos uma vez. Publicar e falhar antes de marcar como publicada faz a mensagem sair duas vezes; o consumidor precisa ser idempotente, e o `MessageId` existe para isso.
- Publique em ordem de gravação e pare no primeiro erro. Pular a mensagem que falhou e seguir para a próxima entrega eventos fora de ordem e insiste contra um broker que já está com problema.
- O relay não pode morrer. Uma exceção não tratada no laço encerra o serviço em segundo plano e todas as mensagens param, silenciosamente.
- Crie índice sobre a consulta do relay (`PublishedAt`, `Id`). Sem ele, cada ciclo vira varredura de tabela inteira, e a tabela só cresce.
- Planeje a limpeza. Mensagens publicadas acumulam para sempre; em produção, um expurgo por idade é parte do padrão, não um extra.
- Com múltiplas instâncias, dois relays leem as mesmas pendentes e publicam em duplicidade. A solução passa por `SELECT ... FOR UPDATE SKIP LOCKED`, lock distribuído ou uma única instância eleita — fora do escopo deste exemplo.
- O padrão adiciona latência: o evento sai no próximo ciclo do relay, não no instante do commit. Aqui são 2 segundos; ajuste conforme o requisito.

## Conteúdo complementar

Endpoints:

| Rota | Papel |
|---|---|
| `POST /orders` | Caminho correto: pedido e mensagem na mesma transação |
| `POST /orders/dual-write` | Caminho errado, para comparação |
| `GET /orders` | Pedidos gravados |
| `GET /outbox` | Mensagens, com estado, tentativas e último erro |
| `GET /published` | O que realmente chegou ao broker |
| `POST /simulation/broker` | Liga e desliga o broker |

O problema, nas duas ordens possíveis:

```text
grava no banco  -> OK
publica         -> FALHA     => pedido existe, ninguem sabe

publica         -> OK
grava no banco  -> FALHA     => evento de um pedido que nao existe
```

Resultado observado com o broker fora do ar:

| Pedido | Caminho | Resposta | Pedido no banco | Evento |
|---|---|---|---|---|
| ana | outbox (broker no ar) | 200 | sim | publicado |
| bruno | dual write | 500 | **sim** | **perdido** |
| carla | dual write | 500 | **sim** | **perdido** |
| diego | outbox | 200 | sim | pendente |

Os pedidos de `bruno` e `carla` ficam no banco sem que evento nenhum exista em lugar nenhum — nem na outbox, nem no broker. Não há como recuperá-los depois, porque não ficou registro de que deveriam ter sido publicados.

Depois de religar o broker, a mensagem de `diego` é publicada sozinha, sem intervenção, e a outbox fica inteira como `publicada`.

Garantias e o que elas custam:

| | Dual write | Outbox |
|---|---|---|
| Perde mensagem | Sim, silenciosamente | Não |
| Duplica mensagem | Não | Sim, em falha do relay |
| Latência de publicação | Imediata | Até um ciclo do relay |
| Custo de infraestrutura | Nenhum | Tabela, índice e expurgo |
| Exige consumidor idempotente | Não | Sim |

Relação com os vizinhos da trilha: `Kafka` e `RabbitMQ` cobrem produção e consumo básicas. Este projeto trata do problema anterior a isso — como garantir que a mensagem chegue a ser produzida. O `TransactionalInboxDemo` do backlog é a contraparte no lado do consumidor.

## Referências e documentação complementar

- https://microservices.io/patterns/data/transactional-outbox.html
- https://learn.microsoft.com/azure/architecture/best-practices/transactional-outbox-cosmos
- https://learn.microsoft.com/ef/core/saving/transactions
