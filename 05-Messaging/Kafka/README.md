# Kafka - Producers & Consumers

## Visão geral

Projeto didático demonstrando publicação e consumo de eventos no Apache Kafka com .NET 9.

## Conceitos abordados

- **Producer/Consumer**: Publicação e consumo de mensagens
- **Topics & Partitions**: Organização de mensagens
- **Consumer Groups**: Balanceamento de carga
- **Offsets**: Rastreamento de posição
- **Acks & Idempotência**: Garantias de entrega

## Objetivos de aprendizagem

- Configurar cluster Kafka local com Docker
- Produzir e consumir mensagens básicas
- Aplicar configurações de confiabilidade
- Entender fluxo de entrega de mensagens

## Estrutura do projeto

```text
Kafka/
+-- Kafka/
|   \-- Receive/
+-- Receive/
|   +-- Program.cs
|   \-- Receive.csproj
+-- Send/
|   +-- Program.cs
|   \-- Send.csproj
+-- docker-compose.yml
\-- Kafka.sln
```

## Como executar

Escolha um dos projetos abaixo para execução direcionada:

- `dotnet run --project 05-Messaging/Kafka/Receive/Receive.csproj`
- `dotnet run --project 05-Messaging/Kafka/Send/Send.csproj`

## Boas práticas e pontos de atenção

| Tema | Recomendação |
|------|--------------|
| **Throughput** | Ajustar `BatchSize`, `LingerMs`, compressão |
| **Confiabilidade** | Usar `Acks=All`, retries com backoff |
| **Commit** | Desabilitar auto-commit em produção |
| **Erros** | Diferenciar falhas transitórias vs permanentes |
| **Observabilidade** | Monitorar offsets, lag, latência |

### Pontos de Atenção

- Verificar porta 9092 disponível
- Auto-commit desabilitado para maior controle
- Consumer deve iniciar antes do producer para demo

## Conteúdo complementar

##### Estrutura do Projeto

```
Kafka/
├── docker-compose.yml    # Kafka + Zookeeper
├── Send/                 # Producer
└── Receive/              # Consumer
```

##### 1. Subir Infraestrutura

```bash
cd Kafka
docker compose up -d
```

Kafka disponível em: `localhost:9092`

##### Producer Básico

```csharp
ProducerConfig config = new ProducerConfig 
{ 
    BootstrapServers = "localhost:9092" 
};

using IProducer<Null, string> producer = 
    new ProducerBuilder<Null, string>(config).Build();

DeliveryResult<Null, string> result = await producer.ProduceAsync(
    "test-topic", 
    new Message<Null, string> { Value = "Hello Kafka!" }
);
```

##### Consumer Básico

```csharp
ConsumerConfig config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "demo-group",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using IConsumer<Ignore, string> consumer = 
    new ConsumerBuilder<Ignore, string>(config).Build();

consumer.Subscribe("test-topic");

while (true)
{
    ConsumeResult<Ignore, string> record = consumer.Consume();
    Console.WriteLine(record.Message.Value);
}
```

##### Próximos Passos

- Adicionar Schema Registry + Avro/Protobuf
- Implementar retry topic / dead letter
- Métricas Prometheus
- Políticas de compactação

## Referências

- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent .NET Client](https://docs.confluent.io/clients-confluent-kafka-dotnet/current/overview.html)
