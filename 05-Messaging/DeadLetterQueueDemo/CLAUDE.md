# CLAUDE.md — DeadLetterQueueDemo

Consumidor RabbitMQ com retentativa limitada, atraso por TTL, dead-letter queue e replay controlado. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 05-Messaging/DeadLetterQueueDemo/DeadLetterQueueDemo.csproj
dotnet run --project 05-Messaging/DeadLetterQueueDemo/DeadLetterQueueDemo.csproj
```

**Exige RabbitMQ** em `localhost:5672`:

```bash
docker run -d --name csharp-codebase-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Sobe em `http://localhost:5054` e declara a topologia sozinho. Painel em `http://localhost:15672` (guest/guest). Roteiro em `DeadLetterQueueDemo.http`.

## Estrutura interna

`Messaging/RabbitMqConnection.DeclareTopologyAsync` é o centro do exemplo. Três filas com papéis distintos:

- `orders.main` → `x-dead-letter-exchange` aponta para `orders.dlx`.
- `orders.retry.queue` → **sem consumidor**, com `x-message-ttl` de 3s e DLX apontando **de volta** para `orders`. A mensagem espera e a expiração a devolve. É assim que se obtém atraso sem plugin.
- `orders.dlq` → sem TTL e sem DLX; ponto final.

`Processing/OrderConsumer.HandleMessageAsync` tem a política inteira, em quatro casos de `switch`. **Note que nenhum deles usa `BasicNack(requeue: true)`** — a mensagem é copiada para a fila de espera e o original recebe `ack`. Reintroduzir `requeue: true` recria o ciclo infinito que o projeto existe para evitar.

`Processing/OrderProcessor` separa `TransientFailure` de `PermanentFailure`. Permanente vai direto para a DLQ com uma tentativa só.

## Pontos de atenção

- TFM `net10.0`. Pacote: `RabbitMQ.Client` 7.2.2 (API assíncrona da v7: `CreateConnectionAsync`, `BasicPublishAsync`, `AsyncEventingBasicConsumer`).
- **Serviço externo obrigatório:** RabbitMQ. Sem ele a aplicação sobe, mas o consumidor falha ao abrir o canal.
- **Armadilha já corrigida uma vez, não reintroduzir:** `OrderProcessor` desserializa com `JsonSerializerDefaults.Web`. Sem isso, o payload camelCase não casa com as propriedades PascalCase, todos os campos vêm vazios e **toda** mensagem vira falha permanente — sem erro de JSON, porque o JSON é válido. O sintoma é a DLQ encher com "mensagem sem orderId".
- **Segunda armadilha já corrigida:** o endpoint `GET /dlq` lê todas as mensagens antes de devolver qualquer uma. Fazer `BasicGet` + `BasicNack(requeue: true)` no mesmo laço recoloca a mensagem na fila na hora, o próximo `BasicGet` a pega de novo, e a requisição nunca termina.
- `BasicQos(prefetchCount: 1)` é necessário para o exemplo ser legível. Sem ele o broker despeja a fila e a ordem das tentativas vira ruído.
- Os números 3 tentativas / 3000ms aparecem em `QueueTopology` **e** no README (tabela e trecho de saída). Alterar um exige atualizar o outro.
- As filas são **duráveis** e sobrevivem ao restart da aplicação. Ao repetir testes, comece com `DELETE /dlq` e `DELETE /attempts`, ou os números não batem com os do README.
- Alterar argumentos de fila (TTL, DLX) com as filas já declaradas provoca `PRECONDITION_FAILED` no start. Nesse caso, apague as filas pelo painel ou recrie o container.
- Backoff exponencial **não** está implementado: exigiria uma fila de espera por faixa de atraso ou o plugin de delayed message. Está citado no README como o passo seguinte; não é omissão acidental.
- **Fronteira com os vizinhos**: `RabbitMQ/Send` e `RabbitMQ/Receive` cobrem o básico de publicar e consumir. Aqui o assunto é só a política de falha. [TransactionalOutboxDemo](../TransactionalOutboxDemo/CLAUDE.md) cuida do lado da produção.
