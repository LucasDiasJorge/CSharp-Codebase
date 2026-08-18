# CLAUDE.md — Kafka/Send (produtor)

Console produtor Kafka. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
# 1) suba o broker (compose na pasta do sample):
cd 05-Messaging/Kafka && docker compose up -d

# 2) produtor:
dotnet run --project 05-Messaging/Kafka/Send/Send.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): configura `ProducerConfig`, publica mensagens no tópico e aguarda a confirmação de entrega. Serilog registra o resultado de cada publicação.

Par com `05-Messaging/Kafka/Receive` — os dois compartilham broker e nome de tópico, e só fazem sentido executados juntos.

## Pontos de atenção

- **Exige Kafka ativo** via `05-Messaging/Kafka/docker-compose.yml`. Sem broker, a execução falha na conexão.
- TFM `net9.0`. `Confluent.Kafka` 2.9.0, Serilog 4.2.0.
- **Sem README local** — a documentação do par produtor/consumidor está em `05-Messaging/Kafka/`.
- Bootstrap servers e tópico estão fixos no código; alterar aqui exige alterar em `Receive` também.
