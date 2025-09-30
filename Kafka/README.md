<!-- README padronizado (versão condensada) -->
# Kafka (.NET Producers & Consumers)

Projeto didático (.NET 9) demonstrando publicação e consumo de eventos no Apache Kafka utilizando a biblioteca oficial Confluent.Kafka. Ênfase em tipos explícitos (sem `var`) e boas práticas de resiliência.

## 1. Visão Geral
Dois subprojetos principais: `Send` (producer) e `Receive` (consumer). Docker Compose sobe Kafka + Zookeeper localmente. Objetivo: entender tópicos, offsets, partitions, consumer groups e fluxo básico de entrega.

## 2. Objetivos Didáticos
- Configurar rapidamente cluster local.
- Produzir e consumir mensagens simples.
- Ilustrar ajustes de confiabilidade (acks, retries, commit manual).
- Introduzir logs / métricas iniciais e idempotência.

## 3. Estrutura Simplificada
```
Kafka/
  docker-compose.yml
  Send/    (Producer mínimo + exemplos avançidos)
  Receive/ (Consumer básico e extensível)
```

## 4. Subir Infra Local
```powershell
cd Kafka
docker compose up -d
```
Verificar porta padrão: `localhost:9092`.

## 5. Executar Producer e Consumer
```powershell
# Terminal 1
cd Send; dotnet run

# Terminal 2
cd ..\Receive; dotnet run
```
Producer envia mensagens para tópico (ex.: `test-topic`), consumer imprime o valor no console.

## 6. Producer Básico (Exemplo)
```csharp
ProducerConfig config = new ProducerConfig { BootstrapServers = "localhost:9092" };
using IProducer<Null, string> producer = new ProducerBuilder<Null, string>(config).Build();
DeliveryResult<Null, string> result = await producer.ProduceAsync("test-topic", new Message<Null, string>{ Value = "Hello Kafka!" });
```

## 7. Consumer Básico (Exemplo)
```csharp
ConsumerConfig config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "demo-group",
    AutoOffsetReset = AutoOffsetReset.Earliest
};
using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build();
consumer.Subscribe("test-topic");
while (true)
{
    ConsumeResult<Ignore, string> record = consumer.Consume();
    Console.WriteLine(record.Message.Value);
}
```

## 8. Boas Práticas Essenciais
| Tema | Recomendações |
|------|---------------|
| Throughput | Ajustar `BatchSize`, `LingerMs`, compressão (Snappy). |
| Confiabilidade | Usar `Acks=All`, retries com backoff, idempotência se necessário. |
| Commit | Desabilitar auto-commit e aplicar commit após processamento. |
| Erros | Diferenciar falhas transitórias vs permanentes (log + alerta). |
| Observabilidade | Registrar offsets, lag, latência e contagem de falhas. |

## 9. Mensagens Enriquecidas
Headers podem transportar tipo de evento e versão:
```csharp
Headers headers = new Headers
{
    { "event-type", Encoding.UTF8.GetBytes("OrderCreated") },
    { "version", Encoding.UTF8.GetBytes("1.0") }
};
```
Facilita roteamento e evolução de schema.

## 10. Idempotência (Esboço)
```csharp
public async Task HandleAsync(string key, string payload)
{
    bool processed = await _store.ExistsAsync(key);
    if (processed) return; // já processado
    await Process(payload);
    await _store.MarkProcessedAsync(key);
}
```

## 11. Docker Compose (Trecho)
Cluster simplificado usando imagens Zookeeper + Kafka. Ajustar se quiser múltiplos brokers, autenticação SASL, TLS ou volumes persistentes.

## 12. Próximos Passos
- Adicionar schema registry + Avro/Protobuf.
- Implementar retry topic / dead letter.
- Métricas Prometheus + exportador. 
- Políticas de compactação para tópicos de estado.

## 13. Referências
- Apache Kafka Documentation
- Confluent .NET Client
- Kafka Patterns & Best Practices

---
Conteúdo original longo substituído por versão condensada padronizada.
