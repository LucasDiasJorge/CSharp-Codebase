using DistributedCacheInvalidationDemo.Cache;
using DistributedCacheInvalidationDemo.Data;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string redisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<NodeCluster>();

WebApplication app = builder.Build();

NodeCluster cluster = app.Services.GetRequiredService<NodeCluster>();
await cluster.SubscribeAllAsync();

// Leitura por um no especifico. A resposta diz de onde o valor veio: L1, L2 ou origem.
app.MapGet("/nodes/{nodeId}/products/{productId}", async (string nodeId, string productId, NodeCluster nodes) =>
{
    CacheNode? node = nodes.Find(nodeId);
    if (node is null)
    {
        return Results.NotFound(new { error = "No desconhecido.", nos = nodes.Nodes.Select(item => item.NodeId) });
    }

    ReadResult result = await node.ReadAsync(productId);

    return result.Value is null
        ? Results.NotFound(new { productId })
        : Results.Ok(new { no = nodeId, valor = result.Value, origem = result.Source });
});

// Escrita COM invalidacao: publica no canal e todos os nos limpam o L1.
app.MapPut("/nodes/{nodeId}/products/{productId}", async (string nodeId, string productId, WriteRequest request, NodeCluster nodes) =>
{
    CacheNode? node = nodes.Find(nodeId);
    if (node is null)
    {
        return Results.NotFound(new { error = "No desconhecido." });
    }

    await node.WriteAsync(productId, request.Value);

    // Pub/Sub e assincrono: a mensagem leva alguns milissegundos para chegar aos outros
    // nos. Essa espera existe so para o exemplo mostrar o estado ja convergido.
    await Task.Delay(100);

    return Results.Ok(new { escritoPor = nodeId, valor = request.Value, invalidacaoPublicada = true });
});

// Escrita SEM invalidacao: os outros nos continuam servindo o valor antigo.
app.MapPut("/nodes/{nodeId}/products/{productId}/no-invalidation", async (string nodeId, string productId, WriteRequest request, NodeCluster nodes) =>
{
    CacheNode? node = nodes.Find(nodeId);
    if (node is null)
    {
        return Results.NotFound(new { error = "No desconhecido." });
    }

    await node.WriteWithoutInvalidationAsync(productId, request.Value);

    return Results.Ok(new { escritoPor = nodeId, valor = request.Value, invalidacaoPublicada = false });
});

// Estado de cada no: e aqui que a divergencia aparece.
app.MapGet("/state", (NodeCluster nodes, ProductRepository repository) => Results.Ok(new
{
    nos = nodes.Nodes.Select(node => new
    {
        no = node.NodeId,
        cacheLocal = node.Snapshot(),
        acertosL1 = node.LocalHits,
        acertosL2 = node.RedisHits,
        leiturasNaOrigem = node.OriginReads,
        invalidacoesRecebidas = node.InvalidationsReceived,
        invalidacoesProprias = node.InvalidationsIgnored
    }),
    leiturasTotaisNoBanco = repository.ReadCount
}));

// Aquece o L1 de todos os nos, para que a divergencia seja visivel depois da escrita.
app.MapPost("/warmup/{productId}", async (string productId, NodeCluster nodes) =>
{
    List<object> results = new List<object>();

    foreach (CacheNode node in nodes.Nodes)
    {
        ReadResult result = await node.ReadAsync(productId);
        results.Add(new { no = node.NodeId, valor = result.Value, origem = result.Source });
    }

    return Results.Ok(results);
});

app.MapPost("/reset", async (NodeCluster nodes, ProductRepository repository, IConnectionMultiplexer redis) =>
{
    nodes.ClearAllLocal();
    repository.ResetCounter();

    await redis.GetDatabase().KeyDeleteAsync(["produto:1", "produto:2"]);

    return Results.Ok(new { reset = true });
});

app.Run();

internal sealed record WriteRequest(string Value);
