using System.Text.Json.Serialization;
using ApiGatewayAggregationDemo.Downstream;
using ApiGatewayAggregationDemo.Gateway;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DownstreamSimulator>();

// Sem este conversor, System.Text.Json so aceita enum como NUMERO: um corpo com
// {"behavior":"Slow"} responderia 400. Vale nos dois sentidos — tambem faz os
// comportamentos aparecerem por nome nas respostas.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// O gateway fala com os servicos por HTTP de verdade — eles apenas moram no mesmo
// processo para o exemplo rodar com um comando.
builder.Services.AddHttpClient<DashboardAggregator>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Gateway:BaseAddress"] ?? "http://localhost:5298");
});

WebApplication app = builder.Build();

// --- Gateway -------------------------------------------------------------------------

app.MapGet("/customers/{customerId}/dashboard", async (
    string customerId,
    HttpContext context,
    DashboardAggregator aggregator,
    bool parallel = true) =>
{
    // Aceita o correlation id do cliente, ou cria um. Reaproveitar o que veio permite
    // rastrear a chamada desde antes do gateway.
    string correlationId = context.Request.Headers.TryGetValue(DashboardAggregator.CorrelationHeader, out Microsoft.Extensions.Primitives.StringValues incoming)
        && !string.IsNullOrWhiteSpace(incoming)
            ? incoming.ToString()
            : $"cid-{Guid.NewGuid():N}"[..12];

    AggregationResult result = await aggregator.AggregateAsync(customerId, correlationId, parallel, context.RequestAborted);

    context.Response.Headers[DashboardAggregator.CorrelationHeader] = correlationId;

    object payload = new
    {
        correlationId,
        result.CustomerId,
        modo = result.Parallel ? "paralelo" : "sequencial",
        tempoTotalMs = result.TotalElapsedMs,
        completo = result.IsComplete,
        partes = result.Fragments.Select(fragment => new
        {
            fragment.Service,
            ok = fragment.Succeeded,
            fragment.ElapsedMs,
            erro = fragment.Error,
            dados = fragment.Data
        })
    };

    // Resposta parcial continua sendo resposta: 200 com o que deu certo e a indicacao
    // do que faltou. So quando NADA responde e que o gateway devolve erro.
    return result.IsTotalFailure
        ? Results.Json(payload, statusCode: StatusCodes.Status503ServiceUnavailable)
        : Results.Ok(payload);
});

// --- Servicos de tras ----------------------------------------------------------------

app.MapGet("/services/profile/{customerId}", async (string customerId, HttpContext context, DownstreamSimulator simulator) =>
{
    simulator.RecordCorrelation(DownstreamSimulator.Profile, ReadCorrelation(context));

    return await simulator.SimulateAsync(DownstreamSimulator.Profile, context.RequestAborted)
        ? Results.Ok(new { customerId, nome = "Ana Souza", plano = "premium" })
        : Results.Problem("servico de perfil indisponivel", statusCode: StatusCodes.Status500InternalServerError);
});

app.MapGet("/services/orders/{customerId}", async (string customerId, HttpContext context, DownstreamSimulator simulator) =>
{
    simulator.RecordCorrelation(DownstreamSimulator.Orders, ReadCorrelation(context));

    return await simulator.SimulateAsync(DownstreamSimulator.Orders, context.RequestAborted)
        ? Results.Ok(new { customerId, total = 3, ultimoPedido = "PED-119" })
        : Results.Problem("servico de pedidos indisponivel", statusCode: StatusCodes.Status500InternalServerError);
});

app.MapGet("/services/recommendations/{customerId}", async (string customerId, HttpContext context, DownstreamSimulator simulator) =>
{
    simulator.RecordCorrelation(DownstreamSimulator.Recommendations, ReadCorrelation(context));

    return await simulator.SimulateAsync(DownstreamSimulator.Recommendations, context.RequestAborted)
        ? Results.Ok(new { customerId, itens = new[] { "ABC-1", "XYZ-9" } })
        : Results.Problem("servico de recomendacoes indisponivel", statusCode: StatusCodes.Status500InternalServerError);
});

// --- Simulacao -----------------------------------------------------------------------

app.MapPost("/simulation/{service}", (string service, BehaviorRequest request, DownstreamSimulator simulator) =>
{
    return simulator.TrySetBehavior(service, request.Behavior)
        ? Results.Ok(new { service, comportamento = request.Behavior.ToString() })
        : Results.NotFound(new { error = "servico desconhecido", conhecidos = simulator.Behaviors.Keys });
});

app.MapPost("/simulation/reset", (DownstreamSimulator simulator) =>
{
    simulator.Reset();

    return Results.Ok(new { reset = true });
});

// Prova de que o correlation id chegou a cada servico.
app.MapGet("/simulation/correlations", (DownstreamSimulator simulator) => Results.Ok(simulator.ReceivedCorrelationIds));

app.MapGet("/simulation", (DownstreamSimulator simulator) => Results.Ok(new
{
    comportamentos = simulator.Behaviors,
    timeoutPorServicoMs = DashboardAggregator.PerServiceTimeout.TotalMilliseconds
}));

app.Run();

static string? ReadCorrelation(HttpContext context)
{
    return context.Request.Headers.TryGetValue(DashboardAggregator.CorrelationHeader, out Microsoft.Extensions.Primitives.StringValues value)
        ? value.ToString()
        : null;
}

internal sealed record BehaviorRequest(DownstreamBehavior Behavior);
