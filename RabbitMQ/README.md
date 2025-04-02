# RabbitMQ Tutorial - "Hello World!" (C#/.NET)

## Overview

This tutorial demonstrates a basic "Hello World!" messaging example using RabbitMQ with C#/.NET. It includes a message producer (sender) and consumer (receiver) that communicate via a RabbitMQ queue.

## Prerequisites

Before you begin, ensure that you have the following installed:

- **RabbitMQ**: Installed and running (default: `localhost:5672`)
- **.NET Core SDK**: Installed on your system
- **Docker** (optional): To run RabbitMQ in a container for ease of setup

## RabbitMQ Setup (Docker Option)

You can run RabbitMQ using Docker with the following commands:

```bash
docker pull rabbitmq:management
docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=usuario \
  -e RABBITMQ_DEFAULT_PASS=senha \
  rabbitmq:management
```

This will set up RabbitMQ with the default user `usuario` and password `senha`. The RabbitMQ management dashboard will be accessible at `http://localhost:15672`.

## Project Structure

Your project will consist of two parts:

### **Send** - Message Producer (Publisher)
- Creates a connection to RabbitMQ
- Declares a queue named `hello`
- Publishes a "Hello World!" message to the queue

### **Receive** - Message Consumer (Receiver)
- Creates a connection to RabbitMQ
- Declares the same `hello` queue
- Listens for messages and prints them to the console

## Key Concepts

- **Producer**: Application that sends messages to a queue
- **Consumer**: Application that receives messages from a queue
- **Queue**: A buffer that holds messages until they are processed
- **Message**: The binary data sent between the producer and consumer

## How to Run

### Step 1: Create the Projects

Run the following commands to create two .NET console applications for the producer and the consumer:

```bash
dotnet new console --name Send
dotnet new console --name Receive
```

### Step 2: Add the RabbitMQ.Client Package

Add the `RabbitMQ.Client` package to both projects:

```bash
cd Send && dotnet add package RabbitMQ.Client
cd ../Receive && dotnet add package RabbitMQ.Client
```

### Step 3: Run the Consumer

First, run the consumer (receiver) application:

```bash
cd Receive && dotnet run
```

### Step 4: Run the Producer

Next, run the producer (sender) application:

```bash
cd Send && dotnet run
```

## Code Highlights

### **Producer** (Send.cs)

The producer creates a connection to RabbitMQ, declares a queue named `hello`, and publishes the "Hello World!" message.

```csharp
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "hello",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

const string message = "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

await channel.BasicPublishAsync(
    exchange: string.Empty, 
    routingKey: "hello", 
    body: body);
```

### **Consumer** (Receive.cs)

The consumer listens for messages from the `hello` queue and prints the received message to the console.

```csharp
var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}");
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync("hello", autoAck: true, consumer: consumer);
```

## Troubleshooting

If you encounter any issues, consider the following troubleshooting steps:

1. **Verify RabbitMQ is running**: Ensure RabbitMQ is up and accessible at `localhost:5672`.
2. **Check connection settings**: Ensure the correct hostname, port, and credentials are provided in your code.
3. **Ensure sufficient disk space**: RabbitMQ requires at least 50MB of free disk space.
4. **Check RabbitMQ logs**: If RabbitMQ is running but not behaving as expected, check its logs for error messages.

## Additional Resources

For additional help, visit the following resources:

- [GitHub Discussions](https://github.com/rabbitmq/rabbitmq-server/discussions)
- [RabbitMQ Community Discord](https://www.rabbitmq.com/community.html)
- [RabbitMQ tutorial](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet)

---
