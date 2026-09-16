using Confluent.Kafka;
using KafkaOrderingDemo.Kafka;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<KafkaSettings>();
builder.Services.AddSingleton<OrderProducer>();
builder.Services.AddSingleton<GroupConsumerRunner>();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    OrderProducer producer = scope.ServiceProvider.GetRequiredService<OrderProducer>();
    await producer.EnsureTopicAsync(CancellationToken.None);
}

// Publica uma sequencia COM chave. Mesma chave, mesma particao, ordem preservada.
app.MapPost("/produce/keyed", async (KeyedRequest request, OrderProducer producer) =>
{
    List<object> results = new List<object>();

    for (int step = 1; step <= request.Count; step++)
    {
        string value = $"{request.Key}:evento-{step}";
        DeliveryResult<string?, string> result = await producer.ProduceAsync(request.Key, value, CancellationToken.None);

        results.Add(new { value, partition = result.Partition.Value, offset = result.Offset.Value });
    }

    return Results.Ok(new
    {
        key = request.Key,
        results,
        nota = "Todas na mesma particao: a chave decide, entao a ordem esta garantida."
    });
});

// Publica SEM chave. O produtor distribui entre as particoes e a ordem relativa se perde.
app.MapPost("/produce/keyless", async (KeylessRequest request, OrderProducer producer) =>
{
    List<object> results = new List<object>();

    for (int step = 1; step <= request.Count; step++)
    {
        string value = $"sem-chave:evento-{step}";
        DeliveryResult<string?, string> result = await producer.ProduceAsync(null, value, CancellationToken.None);

        results.Add(new { value, partition = result.Partition.Value, offset = result.Offset.Value });
    }

    return Results.Ok(new
    {
        results,
        nota = "Espalhadas entre particoes: nao ha ordem garantida entre elas."
    });
});

// Sobe N consumidores no mesmo grupo e relata quem ficou com o que.
app.MapPost("/consume", async (ConsumeRequest request, GroupConsumerRunner runner) =>
{
    GroupRunReport report = await runner.RunAsync(
        request.Group,
        request.Consumers,
        TimeSpan.FromSeconds(request.Seconds),
        CancellationToken.None);

    return Results.Ok(new
    {
        report.GroupId,
        report.ConsumerCount,
        report.PartitionCount,
        ociosos = report.Consumers.Count(consumer => consumer.IsIdle),
        consumidores = report.Consumers.Select(consumer => new
        {
            consumer.ConsumerIndex,
            particoes = consumer.AssignedPartitions,
            mensagens = consumer.MessageCount,
            ocioso = consumer.IsIdle,
            amostra = consumer.Records.Take(12).Select(record => new
            {
                record.Partition,
                record.Offset,
                record.Key,
                record.Value
            })
        })
    });
});

// Verifica se a ordem por chave se manteve: para cada chave, os valores devem aparecer
// na sequencia em que foram produzidos.
app.MapPost("/consume/check-order", async (ConsumeRequest request, GroupConsumerRunner runner) =>
{
    GroupRunReport report = await runner.RunAsync(
        request.Group,
        request.Consumers,
        TimeSpan.FromSeconds(request.Seconds),
        CancellationToken.None);

    List<ConsumedRecord> all = new List<ConsumedRecord>();
    foreach (ConsumerReport consumer in report.Consumers)
    {
        all.AddRange(consumer.Records);
    }

    // Agrupa por chave e confere se os offsets dentro de cada particao crescem junto com
    // a sequencia produzida.
    List<object> perKey = new List<object>();
    foreach (IGrouping<string?, ConsumedRecord> group in all.Where(record => record.Key is not null).GroupBy(record => record.Key))
    {
        List<ConsumedRecord> ordered = group.OrderBy(record => record.Offset).ToList();
        IReadOnlyList<int> partitions = ordered.Select(record => record.Partition).Distinct().ToList();

        bool sequential = true;
        for (int index = 0; index < ordered.Count; index++)
        {
            if (!ordered[index].Value.EndsWith($"evento-{index + 1}", StringComparison.Ordinal))
            {
                sequential = false;
                break;
            }
        }

        perKey.Add(new
        {
            chave = group.Key,
            particoes = partitions,
            mensagens = ordered.Count,
            ordemPreservada = sequential && partitions.Count == 1,
            valores = ordered.Select(record => record.Value)
        });
    }

    return Results.Ok(new { porChave = perKey });
});

// Metadados do topico.
app.MapGet("/topic", (KafkaSettings settings) =>
{
    using IAdminClient admin = new AdminClientBuilder(new AdminClientConfig
    {
        BootstrapServers = settings.BootstrapServers
    }).Build();

    Metadata metadata = admin.GetMetadata(settings.Topic, TimeSpan.FromSeconds(10));
    TopicMetadata topic = metadata.Topics[0];

    return Results.Ok(new
    {
        topic.Topic,
        particoes = topic.Partitions.Count,
        detalhes = topic.Partitions.Select(partition => new { partition.PartitionId, partition.Leader })
    });
});

app.Run();

internal sealed record KeyedRequest(string Key, int Count = 5);

internal sealed record KeylessRequest(int Count = 6);

internal sealed record ConsumeRequest(string Group, int Consumers = 1, int Seconds = 8);
