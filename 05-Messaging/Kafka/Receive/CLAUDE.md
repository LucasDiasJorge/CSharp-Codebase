# CLAUDE.md — Kafka/Receive (consumidor)

Console consumidor Kafka. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
# 1) suba o broker:
cd 05-Messaging/Kafka && docker compose up -d

# 2) consumidor (deixe rodando):
dotnet run --project 05-Messaging/Kafka/Receive/Receive.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): `ConsumerConfig` com consumer group e política de offset, laço de `Consume` bloqueante e encerramento por cancelamento.

O **consumer group** é o detalhe que importa: ele determina onde a leitura recomeça após restart e como as partições são distribuídas entre instâncias.

## Pontos de atenção

- **Exige Kafka ativo** via `05-Messaging/Kafka/docker-compose.yml`.
- O processo **fica em laço aguardando mensagens** — não termina sozinho. Em validação automatizada use `dotnet build`.
- TFM `net9.0`. `Confluent.Kafka` 2.9.0, Serilog 4.2.0.
- **Sem README local** — documentação em `05-Messaging/Kafka/`. Precisa do par `Send` para ter o que consumir.
