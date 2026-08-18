# CLAUDE.md — RabbitMQ/Send (produtor)

Console produtor RabbitMQ. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
# 1) suba o broker:
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management

# 2) produtor:
dotnet run --project 05-Messaging/RabbitMQ/Send/Send.csproj
```

Painel de gestão em `http://localhost:15672` (guest/guest) — útil para ver a fila e as mensagens.

## Estrutura interna

O arquivo é `Send.cs`, **não** `Program.cs`. Fluxo: abrir conexão e canal, declarar a fila (idempotente) e publicar. A declaração da fila no produtor é o que permite executá-lo antes do consumidor.

## Pontos de atenção

- **Exige RabbitMQ ativo** em `localhost:5672`.
- TFM `net9.0`. `RabbitMQ.Client` **7.1.2** — a linha 7.x tem API assíncrona e quebra em relação a exemplos 6.x encontrados na internet. Siga a API do código existente ao editar.
- **Sem README local** — documentação em `05-Messaging/RabbitMQ/`.
