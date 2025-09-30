docker run -d \
dotnet new console --name Send
dotnet new console --name Receive
<!-- README padronizado (versão condensada) -->
# RabbitMQ (Hello Queue)

Exemplo mínimo de mensageria com RabbitMQ em .NET: publisher (`Send`) envia mensagem para fila `hello`, consumer (`Receive`) lê e imprime. Objetivo: ilustrar conexão, declaração de fila idempotente e consumo básico.

## 1. Infra Rápida (Docker)
```powershell
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 `
    -e RABBITMQ_DEFAULT_USER=usuario -e RABBITMQ_DEFAULT_PASS=senha rabbitmq:management
```
Dashboard: http://localhost:15672  (login: usuario / senha)

## 2. Estrutura
```
RabbitMQ/
    Send/ (publisher)
    Receive/ (consumer)
```

## 3. Dependência
```powershell
dotnet add package RabbitMQ.Client
```

## 4. Execução
```powershell
# Terminal 1 (consumer)
cd Receive; dotnet run

# Terminal 2 (producer)
cd Send; dotnet run
```

## 5. Producer (Essência)
```csharp
ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
using IConnection connection = factory.CreateConnection();
using IChannel channel = connection.CreateChannel();
await channel.QueueDeclareAsync("hello", false, false, false, null);
byte[] body = Encoding.UTF8.GetBytes("Hello World!");
await channel.BasicPublishAsync(string.Empty, "hello", body: body);
```

## 6. Consumer (Essência)
```csharp
AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (_, ea) =>
{
        string message = Encoding.UTF8.GetString(ea.Body.ToArray());
        Console.WriteLine(message);
        return Task.CompletedTask;
};
await channel.BasicConsumeAsync("hello", true, consumer);
```

## 7. Conceitos Chave
- Queue: buffer durável (neste exemplo não-durável para simplicidade).
- Idempotência: declarar fila repetidamente é seguro.
- AutoAck: simplificado; em produção preferir ack manual para garantir processamento.

## 8. Próximos Passos
- Tornar fila durável (`durable: true`).
- Mensagens persistentes (`IBasicProperties.Persistent = true`).
- Acks manuais + políticas de retry / DLQ.
- Exchanges + routing keys (fanout / topic / direct).

## 9. Troubleshooting Rápido
| Problema | Ação |
|----------|------|
| Conexão recusa | Verificar container ativo / porta 5672 |
| Sem mensagens | Confirmar publisher executou depois do consumer |
| Dashboard inacessível | Conferir porta 15672 e credenciais |

---
Versão condensada do tutorial original.
