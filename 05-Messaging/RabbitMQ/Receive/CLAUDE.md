# CLAUDE.md — RabbitMQ/Receive (consumidor)

Console consumidor RabbitMQ. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
# 1) suba o broker:
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management

# 2) consumidor (deixe rodando):
dotnet run --project 05-Messaging/RabbitMQ/Receive/Receive.csproj
```

## Estrutura interna

O arquivo é `Receive.cs`, **não** `Program.cs`. Registra um consumer por evento no canal e aguarda; a fila é declarada aqui também, de modo que a ordem de inicialização entre produtor e consumidor não importa.

## Pontos de atenção

- **Exige RabbitMQ ativo** em `localhost:5672`.
- O processo **não termina sozinho** — fica aguardando mensagens. Em validação automatizada use `dotnet build`.
- TFM `net9.0`. `RabbitMQ.Client` 7.1.2 (API assíncrona da linha 7.x).
- **Sem README local** — documentação em `05-Messaging/RabbitMQ/`. Precisa do par `Send`.
