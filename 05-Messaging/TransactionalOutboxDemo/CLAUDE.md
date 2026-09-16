# CLAUDE.md — TransactionalOutboxDemo

API que grava entidade e mensagem na mesma transação e publica depois por um relay, eliminando o dual write. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 05-Messaging/TransactionalOutboxDemo/TransactionalOutboxDemo.csproj
dotnet run --project 05-Messaging/TransactionalOutboxDemo/TransactionalOutboxDemo.csproj
```

**Exige RabbitMQ** em `localhost:5672`:

```bash
docker run -d --name csharp-codebase-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Sobe em `http://localhost:5260`. Roteiro em `TransactionalOutboxDemo.http`; o README traz a sequência em curl.

## Estrutura interna

`Program.cs` tem os dois caminhos lado a lado de propósito: `POST /orders` (outbox) e `POST /orders/dual-write` (o erro). **Não remover o segundo** — é a metade do exemplo que mostra o problema.

**O ponto mais delicado está em `POST /orders`:** há uma transação explícita (`BeginTransactionAsync`) envolvendo os dois `SaveChangesAsync`. O primeiro só existe para obter o `Id` do pedido; sem a transação, seriam dois commits separados e o dual write voltaria por dentro do próprio banco. Uma versão anterior deste sample tinha exatamente esse defeito.

`Outbox/OrdersDbContext` mantém `Orders` e `OutboxMessages` no **mesmo** `DbContext` e banco — condição do padrão, não organização.

`Outbox/OutboxRelay` publica em ordem de `Id` e **para no primeiro erro** (`break`), para não quebrar ordem nem insistir contra broker em falha. O laço captura exceção para nunca morrer: relay morto = mensagens paradas em silêncio.

`Messaging/RabbitMqPublisher` tem `SimulatedOutage`, que é o que torna o cenário de falha observável sem derrubar o container.

## Pontos de atenção

- TFM `net10.0`. Pacotes: `Microsoft.EntityFrameworkCore.Sqlite` 10.0.12 e `RabbitMQ.Client` 7.2.2 (API assíncrona da v7: `CreateConnectionAsync`, `BasicPublishAsync`).
- **Serviço externo obrigatório:** RabbitMQ. Sem ele, a aplicação sobe e a outbox acumula pendentes — o que, convenientemente, é o próprio cenário de broker fora do ar.
- SQLite criado por `EnsureCreatedAsync` em `outbox-demo.db`, ignorado pelo `.gitignore` local. Não versionar o banco (o vizinho `TransactionalOrderApi` versiona o dele; não copiar esse hábito).
- O intervalo do relay é de 2s (`PollInterval`) e o lote é de 20 (`BatchSize`). O README cita "até 2 segundos" — alterar um exige atualizar o outro.
- **Limitação conhecida e documentada:** com múltiplas instâncias, dois relays leem as mesmas pendentes e duplicam publicações. Resolver exigiria `SKIP LOCKED` ou lock distribuído, fora do escopo. Está no README; não "consertar" sem pedido, porque a solução completa dobraria o tamanho do exemplo.
- Não há expurgo da tabela de outbox. É mencionado no README como parte do padrão em produção; ausente aqui de propósito.
- **Fronteira com os vizinhos**: `Kafka` e `RabbitMQ` (trilha 05) cobrem produzir e consumir. Este projeto trata de garantir que a mensagem chegue a ser produzida. O lado do consumidor (inbox/idempotência) é ideia separada no backlog.
