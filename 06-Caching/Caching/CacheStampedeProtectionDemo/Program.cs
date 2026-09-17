using System.Diagnostics;
using CacheStampedeProtectionDemo.Caching;
using CacheStampedeProtectionDemo.Origin;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string redisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton<SlowDataSource>();
builder.Services.AddSingleton<CacheStrategies>();

WebApplication app = builder.Build();

// Roda o experimento: limpa a chave, dispara N requisicoes simultaneas e conta quantas
// chegaram a origem. E esse numero que separa uma estrategia da outra.
app.MapPost("/experiment", async (ExperimentRequest request, CacheStrategies cache, SlowDataSource origin) =>
{
    await cache.ClearAsync(request.Key);
    origin.ResetCounter();

    Stopwatch watch = Stopwatch.StartNew();

    Task<string>[] calls = new Task<string>[request.Concurrency];
    for (int index = 0; index < request.Concurrency; index++)
    {
        calls[index] = request.Strategy switch
        {
            "singleflight" => cache.GetSingleFlightAsync(request.Key, CancellationToken.None),
            _ => cache.GetNaiveAsync(request.Key, CancellationToken.None)
        };
    }

    string[] values = await Task.WhenAll(calls);
    watch.Stop();

    return Results.Ok(new
    {
        estrategia = request.Strategy,
        requisicoes = request.Concurrency,

        // O numero que importa: quantas vezes a origem foi consultada.
        chamadasAOrigem = origin.CallCount,

        valoresDistintos = values.Distinct().Count(),
        tempoTotalMs = (int)watch.ElapsedMilliseconds,
        latenciaDaOrigemMs = (int)SlowDataSource.Latency.TotalMilliseconds
    });
});

// Stale-while-revalidate: consulta sucessivas vezes e mostra quando o valor servido era
// velho e quando a revalidacao foi disparada.
app.MapGet("/swr/{key}", async (string key, CacheStrategies cache, SlowDataSource origin) =>
{
    Stopwatch watch = Stopwatch.StartNew();
    StaleResult result = await cache.GetStaleWhileRevalidateAsync(key, CancellationToken.None);
    watch.Stop();

    return Results.Ok(new
    {
        valor = result.Value,
        servidoVelho = result.WasStale,
        revalidacaoDisparada = result.TriggeredRefresh,
        tempoMs = (int)watch.ElapsedMilliseconds,
        chamadasAOrigem = origin.CallCount
    });
});

// Mostra o efeito do jitter sobre os prazos de expiracao.
app.MapGet("/jitter", () =>
{
    List<object> samples = new List<object>();

    for (int index = 0; index < 10; index++)
    {
        samples.Add(new
        {
            semJitter = (int)CacheStrategies.BaseTtl.TotalMilliseconds,
            comJitter = (int)TtlJitter.Apply(CacheStrategies.BaseTtl).TotalMilliseconds
        });
    }

    return Results.Ok(new
    {
        baseMs = (int)CacheStrategies.BaseTtl.TotalMilliseconds,
        amplitude = $"+/- {TtlJitter.JitterFraction:P0}",
        amostras = samples,
        nota = "Sem jitter, chaves populadas juntas expiram juntas — e o stampede volta multiplicado."
    });
});

app.MapPost("/reset", async (ExperimentRequest request, CacheStrategies cache, SlowDataSource origin) =>
{
    await cache.ClearAsync(request.Key);
    origin.ResetCounter();

    return Results.Ok(new { cleared = request.Key });
});

app.MapGet("/stats", (SlowDataSource origin) => Results.Ok(new { chamadasAOrigem = origin.CallCount }));

app.Run();

internal sealed record ExperimentRequest(string Key = "produto:1", string Strategy = "naive", int Concurrency = 30);
