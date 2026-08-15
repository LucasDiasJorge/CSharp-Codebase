# CLAUDE.md — QueueExample

Console sobre a estrutura de dados `Queue<T>` (FIFO). Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 05-Messaging/QueueExample/QueueExample.csproj
dotnet run --project 05-Messaging/QueueExample/QueueExample.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): `Enqueue`, `Dequeue`, `Peek` e iteração, com um cenário de atendimento por ordem de chegada.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- **Não é mensageria**: apesar de estar em `05-Messaging`, aqui não há broker nem processo separado — é a coleção em memória do .NET. Para fila com concorrência real, ver `02-AsyncAndConcurrency/JobQueueDemo` (`Channel<T>`); para broker de verdade, `Kafka/` e `RabbitMQ/` nesta mesma trilha.
