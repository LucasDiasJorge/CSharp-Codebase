# CLAUDE.md — TransactionalInboxDemo

API com consumo idempotente: marca da mensagem e efeito de domínio na mesma transação. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/TransactionalInboxDemo/TransactionalInboxDemo.csproj
dotnet run --project 08-ArchitecturalPatterns/TransactionalInboxDemo/TransactionalInboxDemo.csproj
```

**Exige RabbitMQ** em `localhost:5672`:

```bash
docker run -d --name csharp-codebase-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Sobe em `http://localhost:5012`. Roteiro em `TransactionalInboxDemo.http`.

## Estrutura interna

`Messaging/CreditConsumer` consome **duas filas** ligadas à mesma exchange `fanout`: `inbox-demo.guarded` (com inbox) e `inbox-demo.naive` (sem). O contraste é o exemplo; **não remover a fila ingênua**.

`ProcessWithInboxAsync` é o ponto crítico: `BeginTransactionAsync`, consulta na inbox, crédito, `InboxMessages.Add`, `SaveChangesAsync`, `CommitAsync`. **A marca e o efeito precisam do mesmo commit.** Gravar a marca depois, em outra transação, reabre a janela que o padrão fecha — e o defeito só aparece sob falha, nunca em teste feliz.

`Inbox/InboxDbContext` tem **índice único** em `MessageId`. A consulta prévia cobre reentrega sequencial; duas entregas simultâneas passam pelas duas consultas e só a restrição do banco impede o efeito duplo.

`Inbox` e `Accounts` vivem no **mesmo** `DbContext` e banco — condição do padrão, não organização.

## Pontos de atenção

- TFM `net10.0`. Pacotes: `Microsoft.EntityFrameworkCore.Sqlite` 10.0.12 e `RabbitMQ.Client` 7.2.2.
- **Serviço externo obrigatório:** RabbitMQ. Sem ele o consumidor falha ao abrir o canal no start.
- SQLite em `inbox-demo.db`, ignorado pelo `.gitignore` local. Não versionar.
- Os números do README (100.00 com inbox, 400.00 sem, após 1 crédito + 3 reentregas) dependem de `amount: 100` e `times: 3`. Alterar exige refazer a tabela.
- As filas são **duráveis** e o SQLite persiste. Ao repetir o roteiro, apague `inbox-demo.db` e purgue as filas, ou os saldos acumulam entre execuções e não batem com o README.
- `ProcessWithoutInboxAsync` usa a conta com sufixo `-sem-inbox` para que os dois consumidores não disputem a mesma linha. É andaime do exemplo.
- **Violação do índice único deveria ser tratada como sucesso** (outra execução já processou). Este sample não demonstra esse caminho — a consulta prévia resolve o caso sequencial e não há concorrência real no roteiro. Está citado no README como cuidado; se alguém acrescentar um cenário concorrente, o `catch (DbUpdateException)` precisa existir.
- Não há expurgo da tabela de inbox. Mencionado no README como parte do padrão em produção; ausente aqui de propósito.
- **Fronteira com os vizinhos**: `TransactionalOutboxDemo` (trilha 05) é a contraparte — outbox no produtor, inbox no consumidor. Os dois READMEs se referenciam; ao mexer em um, conferir se a comparação no outro ainda vale. `DeadLetterQueueDemo` cobre falha definitiva de processamento, que é outro problema.
