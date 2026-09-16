using System.Text;
using System.Text.Json;
using DeadLetterQueueDemo.Messaging;
using DeadLetterQueueDemo.Processing;
using RabbitMQ.Client;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddSingleton<OrderProcessor>();
builder.Services.AddSingleton<ProcessingLog>();
builder.Services.AddHostedService<OrderConsumer>();

WebApplication app = builder.Build();

// Publica um pedido. O campo behavior decide o que o consumidor vai fazer com ele.
app.MapPost("/orders", async (PublishOrderRequest request, RabbitMqConnection connection) =>
{
    await using IChannel channel = await connection.CreateChannelAsync(CancellationToken.None);
    await connection.DeclareTopologyAsync(channel, CancellationToken.None);

    string payload = JsonSerializer.Serialize(new
    {
        orderId = request.OrderId,
        behavior = request.Behavior,
        succeedOnAttempt = request.SucceedOnAttempt
    });

    await channel.BasicPublishAsync(
        exchange: QueueTopology.MainExchange,
        routingKey: QueueTopology.RoutingKey,
        mandatory: false,
        basicProperties: new BasicProperties { Persistent = true },
        body: Encoding.UTF8.GetBytes(payload));

    return Results.Ok(new { published = request.OrderId, behavior = request.Behavior });
});

// Publica um payload cru, para testar mensagem malformada.
app.MapPost("/orders/raw", async (RawRequest request, RabbitMqConnection connection) =>
{
    await using IChannel channel = await connection.CreateChannelAsync(CancellationToken.None);
    await connection.DeclareTopologyAsync(channel, CancellationToken.None);

    await channel.BasicPublishAsync(
        exchange: QueueTopology.MainExchange,
        routingKey: QueueTopology.RoutingKey,
        mandatory: false,
        basicProperties: new BasicProperties { Persistent = true },
        body: Encoding.UTF8.GetBytes(request.Payload));

    return Results.Ok(new { published = "raw" });
});

// Profundidade das tres filas.
app.MapGet("/queues", async (RabbitMqConnection connection) =>
{
    await using IChannel channel = await connection.CreateChannelAsync(CancellationToken.None);
    await connection.DeclareTopologyAsync(channel, CancellationToken.None);

    return Results.Ok(new
    {
        main = (await channel.MessageCountAsync(QueueTopology.MainQueue)),
        retry = (await channel.MessageCountAsync(QueueTopology.RetryQueue)),
        dlq = (await channel.MessageCountAsync(QueueTopology.DeadLetterQueue))
    });
});

// Espia a DLQ sem consumir: o motivo de cada mensagem vem no cabecalho.
app.MapGet("/dlq", async (RabbitMqConnection connection) =>
{
    await using IChannel channel = await connection.CreateChannelAsync(CancellationToken.None);
    await connection.DeclareTopologyAsync(channel, CancellationToken.None);

    List<object> messages = new List<object>();
    List<ulong> deliveryTags = new List<ulong>();

    // Le tudo primeiro, sem devolver nada. Enquanto nao houver ack ou nack, as mensagens
    // ficam invisiveis para novos BasicGet no mesmo canal — por isso o laco termina.
    // Devolver dentro do laco (nack com requeue) recolocaria a mensagem na fila na hora
    // e o proximo BasicGet a pegaria de novo, em ciclo infinito.
    while (true)
    {
        BasicGetResult? result = await channel.BasicGetAsync(QueueTopology.DeadLetterQueue, autoAck: false);
        if (result is null)
        {
            break;
        }

        messages.Add(new
        {
            payload = Encoding.UTF8.GetString(result.Body.ToArray()),
            attempt = ReadHeader(result.BasicProperties, QueueTopology.AttemptHeader),
            reason = ReadHeader(result.BasicProperties, QueueTopology.ReasonHeader)
        });

        deliveryTags.Add(result.DeliveryTag);
    }

    // So agora devolve: a DLQ nao pode ser esvaziada por quem so queria olhar.
    foreach (ulong deliveryTag in deliveryTags)
    {
        await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
    }

    return Results.Ok(messages);
});

// Reprocessamento controlado: devolve a DLQ para a fila principal, com a contagem
// zerada. E uma decisao deliberada, tomada depois que a causa foi corrigida — DLQ que
// se reprocessa sozinha e so um jeito mais lento de ter ciclo infinito.
app.MapPost("/dlq/replay", async (RabbitMqConnection connection, ILoggerFactory loggerFactory) =>
{
    ILogger logger = loggerFactory.CreateLogger("Dlq.Replay");

    await using IChannel channel = await connection.CreateChannelAsync(CancellationToken.None);
    await connection.DeclareTopologyAsync(channel, CancellationToken.None);

    int replayed = 0;

    while (true)
    {
        BasicGetResult? result = await channel.BasicGetAsync(QueueTopology.DeadLetterQueue, autoAck: false);
        if (result is null)
        {
            break;
        }

        await channel.BasicPublishAsync(
            exchange: QueueTopology.MainExchange,
            routingKey: QueueTopology.RoutingKey,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: result.Body.ToArray());

        await channel.BasicAckAsync(result.DeliveryTag, multiple: false);
        replayed++;
    }

    logger.LogInformation("{Quantidade} mensagens devolvidas da DLQ para a fila principal.", replayed);

    return Results.Ok(new { replayed });
});

app.MapDelete("/dlq", async (RabbitMqConnection connection) =>
{
    await using IChannel channel = await connection.CreateChannelAsync(CancellationToken.None);
    await connection.DeclareTopologyAsync(channel, CancellationToken.None);

    uint purged = await channel.QueuePurgeAsync(QueueTopology.DeadLetterQueue);

    return Results.Ok(new { purged });
});

// Historico de tentativas do consumidor.
app.MapGet("/attempts", (ProcessingLog log) => Results.Ok(log.GetAll()));

app.MapDelete("/attempts", (ProcessingLog log) =>
{
    log.Clear();

    return Results.Ok(new { cleared = true });
});

app.Run();

static string? ReadHeader(IReadOnlyBasicProperties properties, string name)
{
    if (properties.Headers is null || !properties.Headers.TryGetValue(name, out object? value))
    {
        return null;
    }

    return value switch
    {
        byte[] bytes => Encoding.UTF8.GetString(bytes),
        null => null,
        _ => value.ToString()
    };
}

internal sealed record PublishOrderRequest(string OrderId, string Behavior, int SucceedOnAttempt = 2);

internal sealed record RawRequest(string Payload);
