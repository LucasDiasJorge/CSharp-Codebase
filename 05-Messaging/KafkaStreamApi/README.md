# KafkaStreamApi

## Visão geral

API Web para consumo de mensagens Kafka em tempo real via Server-Sent Events (SSE) e NDJSON streaming.

## Conceitos abordados

- **Kafka Consumer**: Consumo de mensagens de tópicos Kafka usando Confluent.Kafka
- **Server-Sent Events (SSE)**: Streaming unidirecional de dados do servidor para o cliente
- **NDJSON Streaming**: Formato de streaming JSON delimitado por linha
- **IAsyncEnumerable**: Geração de dados assíncronos com yield return
- **Serilog**: Logging estruturado para monitoramento

## Objetivos de aprendizagem

- Implementar consumo de Kafka em uma API ASP.NET Core
- Criar endpoints de streaming com SSE e NDJSON
- Usar IAsyncEnumerable para processamento eficiente de streams
- Configurar logging estruturado com Serilog

## Estrutura do projeto

```text
KafkaStreamApi/
+-- Controllers/
|   \-- KafkaController.cs
+-- KafkaStreamApi/
+-- Properties/
|   \-- launchSettings.json
+-- Services/
|   \-- KafkaConsumerService.cs
+-- appsettings.Development.json
+-- appsettings.json
+-- KafkaStreamApi.csproj
+-- KafkaStreamApi.csproj.user
\-- ...
```

## Como executar

```bash
dotnet run --project 05-Messaging/KafkaStreamApi/KafkaStreamApi.csproj
```

## Boas práticas e pontos de atenção

- Usar CancellationToken para cancelamento gracioso
- Implementar IDisposable no consumer service
- Logging estruturado para debugging de streams
- Validar configurações do Kafka no startup

### Pontos de Atenção

- Configurar `appsettings.json` com BootstrapServers e GroupId
- SSE mantém conexão aberta - considerar timeouts
- Em produção, usar consumer groups para escalabilidade

## Conteúdo complementar

##### Estrutura do Projeto

```
KafkaStreamApi/
├── Controllers/
│   └── KafkaController.cs      # Endpoints de streaming
├── Services/
│   └── KafkaConsumerService.cs # Serviço de consumo Kafka
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── Program.cs
└── README.md
```

##### Pré-requisitos

- .NET 9.0 SDK
- Apache Kafka rodando (Docker recomendado)

##### Configuração do Kafka

```bash
# Subir Kafka com Docker
docker run -d --name kafka -p 9092:9092 \
  -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
  -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 \
  confluentinc/cp-kafka
```

##### Endpoints

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/kafka/stream/{topic}/{groupId}` | GET | Stream SSE de mensagens |
| `/api/kafka/stream-ndjson/{topic}/{groupId}` | GET | Stream NDJSON de mensagens |

##### Exemplo de Uso

```bash
# SSE Stream
curl -N http://localhost:5000/api/kafka/stream/meu-topico/meu-grupo

# NDJSON Stream
curl -N http://localhost:5000/api/kafka/stream-ndjson/meu-topico/meu-grupo
```

##### Consumer Service com IAsyncEnumerable

```csharp
public async IAsyncEnumerable<string> ConsumeMessagesAsStreamAsync(
    string topic, 
    string groupId,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    consumer.Subscribe(topic);

    while (!cancellationToken.IsCancellationRequested)
    {
        ConsumeResult<Ignore, string> consumeResult = consumer.Consume(cancellationToken);
        if (consumeResult?.Message?.Value != null)
        {
            yield return consumeResult.Message.Value;
        }
    }
}
```

##### Controller SSE Endpoint

```csharp
[HttpGet("stream/{topic}/{groupId}")]
public async Task GetKafkaStream(string topic, string groupId, CancellationToken cancellationToken)
{
    Response.ContentType = "text/event-stream";
    
    await foreach (string message in _kafkaService.ConsumeMessagesAsStreamAsync(topic, groupId, cancellationToken))
    {
        string data = $"data: {JsonSerializer.Serialize(message)}\n\n";
        await Response.WriteAsync(data, cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
```

## Referências

- [Confluent Kafka .NET](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
- [Server-Sent Events MDN](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events)
- [IAsyncEnumerable](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/generate-consume-asynchronous-stream)
