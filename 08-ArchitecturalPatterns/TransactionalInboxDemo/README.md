# TransactionalInboxDemo

API que garante consumo idempotente registrando a mensagem recebida e a alteração de domínio na mesma transação.

## Visão geral

Todo broker entrega **pelo menos uma vez**. Não é uma possibilidade remota: basta o ack se perder no caminho, o consumidor reiniciar antes de confirmar, ou o produtor reenviar por não ter recebido confirmação. A mesma mensagem vai chegar duas vezes, e o consumidor precisa estar pronto para isso.

Se a operação for creditar um valor, processar duas vezes deixa um saldo errado — e errado em silêncio, porque nada falhou. O inbox resolve registrando o identificador de cada mensagem já processada: na segunda vez, o consumidor reconhece e ignora.

O detalhe que faz o padrão funcionar é a transação. A marca "já processei esta mensagem" e o efeito no domínio precisam ser commitados juntos. Se forem duas transações, existe uma janela em que o crédito já foi aplicado mas a mensagem ainda parece nova — e uma reentrega nesse intervalo credita de novo, exatamente o que se queria evitar.

O exemplo consome a mesma mensagem por duas filas em paralelo: uma com inbox e outra sem. Reentregando três vezes, a conta protegida fica em 100 e a desprotegida vai a 400.

## Conceitos abordados

- Entrega pelo menos uma vez e por que a duplicata é certa, não improvável.
- Inbox como registro de mensagens já processadas.
- Marca do inbox e efeito de domínio na mesma transação.
- Identificador vindo do produtor, não gerado no consumidor.
- Índice único como garantia real contra entregas concorrentes.
- Idempotência como responsabilidade do consumidor.
- Contrapartida do outbox: um garante a publicação, o outro o consumo.

## Objetivos de aprendizagem

- Reconhecer que consumir mensagem exige idempotência, sempre.
- Implementar o inbox sem reintroduzir a janela que ele existe para fechar.
- Entender por que a consulta prévia não basta e o índice único é necessário.
- Escolher o identificador certo para deduplicação.

## Estrutura do projeto

```text
TransactionalInboxDemo/
|-- Domain/
|   `-- Account.cs
|-- Inbox/
|   |-- InboxDbContext.cs
|   `-- InboxMessage.cs
|-- Messaging/
|   `-- CreditConsumer.cs
|-- Properties/
|   `-- launchSettings.json
|-- Program.cs
|-- README.md
|-- TransactionalInboxDemo.csproj
`-- TransactionalInboxDemo.http
```

## Como executar

Requer RabbitMQ:

```bash
docker run -d --name csharp-codebase-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Depois:

```bash
dotnet run --project 08-ArchitecturalPatterns/TransactionalInboxDemo/TransactionalInboxDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 08-ArchitecturalPatterns/TransactionalInboxDemo/TransactionalInboxDemo.csproj
```

A API sobe em `http://localhost:5012`. O banco SQLite é criado no primeiro start e não é versionado.

Roteiro:

```bash
# publica um credito e guarda o messageId
curl -s -X POST http://localhost:5012/messages/credit \
  -H "Content-Type: application/json" -d '{"accountId":"conta-A","amount":100}'

# reentrega a MESMA mensagem tres vezes
curl -s -X POST http://localhost:5012/messages/redeliver \
  -H "Content-Type: application/json" \
  -d '{"messageId":"<O_MESSAGE_ID>","accountId":"conta-A","amount":100,"times":3}'

curl -s http://localhost:5012/accounts
```

## Boas práticas e pontos de atenção

- Assuma que a duplicata vai acontecer. Entrega exatamente uma vez não existe no transporte; o que existe é entrega pelo menos uma vez com processamento idempotente no consumidor.
- Grave a marca e o efeito na **mesma transação**. Em transações separadas, uma falha entre elas deixa o efeito aplicado e a mensagem parecendo nova — e a reentrega duplica.
- Use o identificador do produtor. Um id gerado no consumidor muda a cada entrega e não reconhece nada; o `MessageId` precisa vir junto da mensagem e ser estável entre reenvios.
- O inbox tem de estar no mesmo banco do domínio. Em outro banco — ou em Redis — volta a ser dual write, com uma janela entre os dois.
- Ponha índice único no identificador. A consulta prévia resolve o caso comum, mas duas entregas simultâneas passam pelas duas consultas antes de qualquer gravação; só a restrição do banco impede o efeito duplo.
- Trate a violação do índice único como sucesso, não como erro. Significa que outra execução já processou a mesma mensagem — o resultado desejado foi alcançado.
- Planeje a limpeza. A tabela cresce com o volume de mensagens; um expurgo por idade faz parte do padrão, com a janela maior que o prazo máximo de reentrega do broker.
- Nem toda operação precisa de inbox. Uma escrita naturalmente idempotente (`SET status = 'pago'`) já tolera repetição; o inbox é necessário quando o efeito se acumula, como somar a um saldo.
- Inbox e outbox são complementares e resolvem pontas diferentes: o outbox garante que a mensagem chegue a ser publicada, o inbox que ela não seja aplicada duas vezes.

## Conteúdo complementar

Endpoints:

| Rota | Finalidade |
|---|---|
| `POST /messages/credit` | Publica um crédito com `MessageId` novo |
| `POST /messages/redeliver` | Reentrega a mesma mensagem N vezes |
| `GET /accounts` | Saldos das duas contas |
| `GET /inbox` | Mensagens já registradas |
| `GET /stats` | Processadas e ignoradas por duplicata |

A exchange é `fanout`: a mesma mensagem chega às duas filas, então a comparação é exata.

```text
                    +--> [inbox-demo.guarded] --> consumidor COM inbox --> conta-A
publica --> [fanout]
                    +--> [inbox-demo.naive]   --> consumidor SEM inbox --> conta-A-sem-inbox
```

Resultado observado — um crédito de 100, depois três reentregas da **mesma** mensagem:

| Conta | Saldo | Créditos aplicados |
|---|---|---|
| `conta-A` (com inbox) | **100.00** | 1 |
| `conta-A-sem-inbox` | **400.00** | 4 |

```json
{ "processadas": 1, "ignoradasPorDuplicata": 3 }
```

O consumidor protegido processou a mensagem uma vez e reconheceu as outras três. O ingênuo aplicou o crédito a cada entrega, e o saldo ficou quatro vezes maior — sem erro, sem log de falha, sem nada que chamasse atenção.

Por que a transação é obrigatória:

```text
duas transacoes separadas:
  t0  aplica o credito     -> commit
  t1  (falha aqui)
  t2  broker reentrega     -> mensagem ainda nao esta na inbox
  t3  aplica o credito DE NOVO

uma transacao:
  t0  aplica o credito + grava a marca -> commit unico
  t1  broker reentrega                 -> marca encontrada, ignora
```

Camadas de proteção, e o que cada uma cobre:

| Mecanismo | Cobre | Não cobre |
|---|---|---|
| Consulta prévia na inbox | Reentrega sequencial | Duas entregas simultâneas |
| Índice único no `MessageId` | Entregas concorrentes | Nada acima disso |
| Transação única | Falha entre efeito e marca | Duplicata em outro serviço |

Inbox e outbox lado a lado:

| | Outbox | Inbox |
|---|---|---|
| Lado | Produtor | Consumidor |
| Garante | Que a mensagem seja publicada | Que não seja aplicada duas vezes |
| Grava junto do domínio | A mensagem a enviar | A marca do que já foi processado |
| Problema que resolve | Perda de evento | Efeito duplicado |

Relação com os vizinhos: `TransactionalOutboxDemo` (trilha 05) é a contraparte exata deste projeto — vale ler os dois em sequência, porque juntos formam a entrega confiável de ponta a ponta. `DeadLetterQueueDemo` trata do que fazer quando o processamento falha de vez.

## Referências e documentação complementar

- https://microservices.io/patterns/data/transactional-outbox.html
- https://www.kamilgrzybek.com/blog/posts/the-outbox-pattern
- https://learn.microsoft.com/azure/architecture/reference-architectures/containers/aks-microservices/aks-microservices-advanced
