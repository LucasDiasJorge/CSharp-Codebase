# RabbitMQ - Hello Queue

## Visão geral

Exemplo mínimo de mensageria com RabbitMQ em .NET 9.

## Conceitos abordados

- **Message Queue**: Filas de mensagens para comunicação assíncrona
- **Producer/Consumer**: Padrão publicador/consumidor
- **Connection/Channel**: Gerenciamento de conexões
- **Queue Declaration**: Criação idempotente de filas
- **Auto Acknowledgement**: Confirmação automática de mensagens

### Conceitos Chave

| Conceito | Descrição |
|----------|-----------|
| **Queue** | Buffer durável para mensagens |
| **Idempotência** | Declarar fila repetidamente é seguro |
| **AutoAck** | Confirmação automática (simplificado) |
| **Exchange** | Roteador de mensagens (não usado neste exemplo básico) |

## Objetivos de aprendizagem

- Configurar ambiente RabbitMQ local com Docker
- Entender conexão, canal e fila
- Publicar e consumir mensagens básicas
- Preparar base para padrões avançados

## Estrutura do projeto

```text
RabbitMQ/
+-- Receive/
|   +-- Receive.cs
|   \-- Receive.csproj
+-- Send/
|   +-- Send.cs
|   \-- Send.csproj
\-- RabbitMQ.sln
```

## Como executar

Escolha um dos projetos abaixo para execução direcionada:

- `dotnet build 05-Messaging/RabbitMQ/Receive/Receive.csproj`
- `dotnet build 05-Messaging/RabbitMQ/Send/Send.csproj`

## Boas práticas e pontos de atenção

- Exemplo usa fila não-durável para simplicidade
- AutoAck ativado (em produção preferir ack manual)
- Não demonstra exchanges ou routing keys

## Conteúdo complementar

##### Estrutura do Projeto

```
RabbitMQ/
├── Send/         # Producer - envia mensagens
└── Receive/      # Consumer - recebe mensagens
```

##### 1. Subir Infraestrutura (Docker)

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 \
    -e RABBITMQ_DEFAULT_USER=usuario -e RABBITMQ_DEFAULT_PASS=senha \
    rabbitmq:management
```

**Dashboard**: http://localhost:15672 (usuario/senha)

##### 2. Instalar Dependência

```bash
dotnet add package RabbitMQ.Client
```

##### Producer (Essência)

```csharp
ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
using IConnection connection = factory.CreateConnection();
using IChannel channel = connection.CreateChannel();

await channel.QueueDeclareAsync("hello", false, false, false, null);
byte[] body = Encoding.UTF8.GetBytes("Hello World!");
await channel.BasicPublishAsync(string.Empty, "hello", body: body);
```

##### Consumer (Essência)

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

##### Próximos Passos

- Tornar fila durável (`durable: true`)
- Mensagens persistentes (`IBasicProperties.Persistent = true`)
- Acks manuais + políticas de retry / DLQ
- Exchanges + routing keys (fanout / topic / direct)

##### Troubleshooting

| Problema | Solução |
|----------|---------|
| Conexão recusada | Verificar container ativo / porta 5672 |
| Sem mensagens | Confirmar producer executou depois do consumer |
| Dashboard inacessível | Conferir porta 15672 e credenciais |

## Referências

- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet.html)
