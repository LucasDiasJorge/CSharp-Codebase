# Apache Kafka com .NET

## 📚 Conceitos Abordados

Este projeto demonstra o uso do Apache Kafka com .NET, incluindo:

- **Message Broker**: Sistema de mensageria distribuída
- **Producer**: Aplicação que envia mensagens
- **Consumer**: Aplicação que consome mensagens
- **Topics**: Canais de comunicação
- **Partitions**: Divisão de dados para escalabilidade
- **Consumer Groups**: Agrupamento de consumidores
- **Docker Compose**: Orquestração de containers

## 🎯 Objetivos de Aprendizado

- Implementar comunicação assíncrona entre serviços
- Configurar e usar Apache Kafka
- Criar producers e consumers eficientes
- Entender conceitos de streaming de dados
- Trabalhar com particionamento e consumer groups
- Usar Docker para desenvolvimento

## 💡 Conceitos Importantes

### Producer (Enviador)
```csharp
var config = new ProducerConfig
{
    BootstrapServers = "localhost:9092"
};

using var producer = new ProducerBuilder<Null, string>(config).Build();

var deliveryResult = await producer.ProduceAsync(
    "test-topic", 
    new Message<Null, string> { Value = "Hello Kafka!" }
);
```

### Consumer (Receptor)
```csharp
var config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "my-group",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
consumer.Subscribe("test-topic");

while (true)
{
    var result = consumer.Consume();
    Console.WriteLine($"Received: {result.Message.Value}");
}
```

## 🚀 Como Executar

### 1. Iniciar Kafka com Docker
```bash
cd Kafka
docker-compose up -d
```

### 2. Executar Consumer (Terminal 1)
```bash
cd Receive
dotnet run
```

### 3. Executar Producer (Terminal 2)
```bash
cd Send
dotnet run
```

## 📖 O que Você Aprenderá

1. **Arquitetura Kafka**:
   - Brokers: Servidores que armazenam mensagens
   - Topics: Categorias de mensagens
   - Partitions: Divisão de topics para paralelismo
   - Offsets: Posição das mensagens

2. **Padrões de Mensageria**:
   - Publish/Subscribe
   - Point-to-Point
   - Request/Response
   - Event Sourcing

3. **Configurações**:
   - Bootstrap servers
   - Consumer groups
   - Auto offset reset
   - Serialization

4. **Garantias de Entrega**:
   - At most once
   - At least once
   - Exactly once

## 🎨 Padrões de Implementação

### 1. Producer Avançado
```csharp
public class OrderEventProducer
{
    private readonly IProducer<string, string> _producer;
    
    public OrderEventProducer(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.GetConnectionString("Kafka"),
            Acks = Acks.All, // Aguarda confirmação de todas as réplicas
            Retries = 3,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            CompressionType = CompressionType.Snappy
        };
        
        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetValueSerializer(Serializers.Utf8)
            .Build();
    }
    
    public async Task PublishOrderCreatedAsync(Order order)
    {
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Amount = order.TotalAmount,
            Timestamp = DateTime.UtcNow
        };
        
        var message = new Message<string, string>
        {
            Key = order.Id.ToString(),
            Value = JsonSerializer.Serialize(orderEvent),
            Headers = new Headers
            {
                { "event-type", Encoding.UTF8.GetBytes("OrderCreated") },
                { "version", Encoding.UTF8.GetBytes("1.0") }
            }
        };
        
        try
        {
            var deliveryResult = await _producer.ProduceAsync("orders", message);
            Console.WriteLine($"Message delivered to {deliveryResult.TopicPartitionOffset}");
        }
        catch (ProduceException<string, string> e)
        {
            Console.WriteLine($"Failed to deliver message: {e.Error.Reason}");
            throw;
        }
    }
}
```

### 2. Consumer Avançado
```csharp
public class OrderEventConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<OrderEventConsumer> _logger;
    
    public OrderEventConsumer(IConfiguration config, ILogger<OrderEventConsumer> logger)
    {
        _logger = logger;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config.GetConnectionString("Kafka"),
            GroupId = "order-processing-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // Commit manual para controle
            SessionTimeoutMs = 6000,
            HeartbeatIntervalMs = 2000,
            MaxPollIntervalMs = 300000
        };
        
        _consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, e) => _logger.LogError($"Error: {e.Reason}"))
            .Build();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("orders");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    
                    if (consumeResult.Message != null)
                    {
                        await ProcessMessage(consumeResult.Message);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Consume error: {e.Error.Reason}");
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }
    
    private async Task ProcessMessage(Message<string, string> message)
    {
        try
        {
            var eventType = Encoding.UTF8.GetString(
                message.Headers.FirstOrDefault(h => h.Key == "event-type")?.GetValueBytes() ?? 
                Array.Empty<byte>()
            );
            
            switch (eventType)
            {
                case "OrderCreated":
                    var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Value);
                    await HandleOrderCreated(orderEvent);
                    break;
                default:
                    _logger.LogWarning($"Unknown event type: {eventType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            // Implementar dead letter queue ou retry logic
        }
    }
}
```

### 3. Configuration e DI
```csharp
// Program.cs
builder.Services.AddSingleton<OrderEventProducer>();
builder.Services.AddHostedService<OrderEventConsumer>();

// appsettings.json
{
  "ConnectionStrings": {
    "Kafka": "localhost:9092"
  },
  "Kafka": {
    "Topics": {
      "Orders": "orders",
      "Payments": "payments"
    }
  }
}
```

## 🏗️ Casos de Uso Comuns

### 1. Event-Driven Architecture
```csharp
// Microserviço publica eventos
await _producer.PublishAsync("user-events", new UserRegisteredEvent
{
    UserId = user.Id,
    Email = user.Email,
    Timestamp = DateTime.UtcNow
});

// Outros serviços reagem aos eventos
// - Email service envia boas-vindas
// - Analytics service registra novo usuário
// - CRM service cria lead
```

### 2. Data Pipeline
```csharp
// Ingestão de dados em tempo real
public class DataPipelineConsumer
{
    public async Task ProcessSensorData(SensorReading reading)
    {
        // Validação
        if (IsValidReading(reading))
        {
            // Transformação
            var transformedData = TransformData(reading);
            
            // Persistência
            await _database.SaveAsync(transformedData);
            
            // Alertas
            if (IsAnomalous(transformedData))
            {
                await _alertService.SendAlertAsync(transformedData);
            }
        }
    }
}
```

### 3. CQRS com Event Sourcing
```csharp
public class OrderAggregate
{
    public async Task ProcessCommand(CreateOrderCommand command)
    {
        var events = new List<DomainEvent>();
        
        // Lógica de negócio
        events.Add(new OrderCreatedEvent(command.OrderId, command.Items));
        
        // Publicar eventos
        foreach (var evt in events)
        {
            await _eventStore.AppendAsync(evt);
            await _eventPublisher.PublishAsync(evt);
        }
    }
}
```

## 🔍 Pontos de Atenção

### Performance
```csharp
// ✅ Configure batch size para melhor throughput
var config = new ProducerConfig
{
    BatchSize = 16384,
    LingerMs = 5,
    CompressionType = CompressionType.Snappy
};

// ✅ Use async para operações I/O
await producer.ProduceAsync(topic, message);

// ✅ Reutilize producers/consumers
// Evite criar nova instância para cada mensagem
```

### Reliability
```csharp
// ✅ Configure retries e timeouts
var config = new ProducerConfig
{
    MessageTimeoutMs = 300000,
    Retries = 3,
    RetryBackoffMs = 1000
};

// ✅ Implemente idempotência
public class IdempotentMessageHandler
{
    public async Task HandleAsync(Message message)
    {
        var messageId = GetMessageId(message);
        if (await _processedMessages.ExistsAsync(messageId))
        {
            return; // Já processada
        }
        
        await ProcessMessage(message);
        await _processedMessages.MarkProcessedAsync(messageId);
    }
}
```

### Monitoring
```csharp
// ✅ Adicione métricas e logs
_logger.LogInformation("Processing message {MessageId} from {Topic}", 
    messageId, topic);

// ✅ Monitor lag do consumer
var lag = consumer.Assignment
    .Sum(tp => consumer.GetWatermarkOffsets(tp).High - consumer.Position(tp));
```

## 🚀 Docker Setup

### docker-compose.yml
```yaml
version: '3.8'
services:
  zookeeper:
    image: zookeeper:3.8
    ports:
      - "2181:2181"
    environment:
      ZOO_MY_ID: 1
      ZOO_SERVERS: server.1=zookeeper:2888:3888;2181

  kafka:
    image: bitnami/kafka:3.4
    ports:
      - "9092:9092"
    environment:
      KAFKA_CFG_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_CFG_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
      ALLOW_PLAINTEXT_LISTENER: "yes"
    depends_on:
      - zookeeper
```

## 📚 Recursos Adicionais

- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent .NET Client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
- [Kafka Best Practices](https://kafka.apache.org/documentation/#bestpractices)
