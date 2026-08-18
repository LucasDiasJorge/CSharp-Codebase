# CLAUDE.md — KafkaStreamApi

Web API que repassa mensagens Kafka ao cliente HTTP em tempo real, via **SSE e NDJSON**. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
# broker (compose vive na pasta do sample Kafka):
cd 05-Messaging/Kafka && docker compose up -d

dotnet run --project 05-Messaging/KafkaStreamApi/KafkaStreamApi.csproj
```

Requisições de exemplo em `KafkaStreamApi.http`.

## Estrutura interna

- `Services/KafkaConsumerService.cs` — consumo contínuo do tópico em background.
- `Controllers/KafkaController.cs` — expõe o stream. O ponto técnico está aqui: a resposta **não é bufferizada**; cada mensagem é escrita e o buffer é liberado imediatamente (`Server-Sent Events` ou NDJSON, uma linha JSON por mensagem).

Qualquer alteração que introduza serialização do payload inteiro antes do envio quebra o streaming e transforma o exemplo numa API comum.

## Pontos de atenção

- **Exige Kafka ativo**; use o compose de `05-Messaging/Kafka/`, este projeto não tem o seu.
- TFM `net9.0`. `Confluent.Kafka` 2.9.0, `Mvc.NewtonsoftJson` **6.0.0** (bem anterior ao TFM — atenção se houver conflito de versão), Serilog 4.2.0.
- Clientes HTTP que bufferizam resposta (incluindo alguns navegadores/proxies) escondem o efeito de streaming.
