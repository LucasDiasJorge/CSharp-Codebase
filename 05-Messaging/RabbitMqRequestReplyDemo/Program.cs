using System.Text.Json;
using RabbitMqRequestReplyDemo.Client;
using RabbitMqRequestReplyDemo.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RpcClient>();
builder.Services.AddHostedService<RpcServer>();

WebApplication app = builder.Build();

// Requisicao com resposta. O timeout e do CLIENTE: o servidor nao sabe nem e avisado
// que alguem desistiu de esperar.
app.MapPost("/rpc/sum", async (SumRequest request, RpcClient client) =>
{
    string payload = JsonSerializer.Serialize(new
    {
        a = request.A,
        b = request.B,
        delayMs = request.DelayMs
    });

    RpcResult result = await client.CallAsync(payload, TimeSpan.FromMilliseconds(request.TimeoutMs), CancellationToken.None);

    if (!result.Answered)
    {
        return Results.Json(new
        {
            answered = false,
            elapsedMs = (int)result.Elapsed.TotalMilliseconds,
            nota = "Timeout do cliente. O servidor pode muito bem responder depois — e essa resposta sera descartada."
        }, statusCode: StatusCodes.Status504GatewayTimeout);
    }

    return Results.Ok(new
    {
        answered = true,
        elapsedMs = (int)result.Elapsed.TotalMilliseconds,
        response = JsonSerializer.Deserialize<JsonElement>(result.Response!)
    });
});

// Varias requisicoes ao mesmo tempo, para mostrar que o CorrelationId e o que mantem
// cada resposta ligada ao seu pedido mesmo com tudo em paralelo.
app.MapPost("/rpc/concurrent", async (ConcurrentRequest request, RpcClient client) =>
{
    List<Task<(int Index, RpcResult Result)>> calls = new List<Task<(int, RpcResult)>>();

    for (int index = 0; index < request.Count; index++)
    {
        int current = index;
        string payload = JsonSerializer.Serialize(new { a = current, b = current, delayMs = request.DelayMs });

        calls.Add(Task.Run(async () =>
        {
            RpcResult result = await client.CallAsync(payload, TimeSpan.FromMilliseconds(request.TimeoutMs), CancellationToken.None);

            return (current, result);
        }));
    }

    (int Index, RpcResult Result)[] results = await Task.WhenAll(calls);

    return Results.Ok(new
    {
        total = results.Length,
        respondidas = results.Count(item => item.Result.Answered),
        expiradas = results.Count(item => !item.Result.Answered),
        detalhes = results.OrderBy(item => item.Index).Select(item => new
        {
            item.Index,

            // Cada resposta deve trazer a soma do proprio indice (index + index).
            // Se o CorrelationId estivesse trocado, apareceria o valor de outro pedido.
            esperado = item.Index + item.Index,
            recebido = item.Result.Answered
                ? JsonSerializer.Deserialize<JsonElement>(item.Result.Response!).GetProperty("result").GetInt32()
                : (int?)null,
            item.Result.Answered
        })
    });
});

// Prova de que a limpeza funciona: pendentes deve voltar a zero mesmo depois de timeouts.
app.MapGet("/pending", (RpcClient client) => Results.Ok(new
{
    pendentes = client.PendingCount,
    respostasTardiasDescartadas = client.LateResponses
}));

app.Run();

internal sealed record SumRequest(int A, int B, int DelayMs = 0, int TimeoutMs = 3000);

internal sealed record ConcurrentRequest(int Count = 8, int DelayMs = 0, int TimeoutMs = 3000);
