using RedisDistributedLockDemo.Locking;
using RedisDistributedLockDemo.Work;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string redisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton<DistributedLock>();
builder.Services.AddSingleton<ProtectedResource>();
builder.Services.AddSingleton<WorkerRunner>();

WebApplication app = builder.Build();

// Cenario principal: N trabalhadores disputando o mesmo lock.
//
// Com autoRenew = false e workMs > ttlMs, o lease expira NO MEIO do trabalho, outro
// trabalhador adquire o lock e os dois ficam na secao critica ao mesmo tempo — a falha
// que este exemplo existe para tornar visivel.
app.MapPost("/scenario", async (ScenarioRequest request, WorkerRunner runner, ProtectedResource resource) =>
{
    resource.Reset();

    IReadOnlyList<WorkerOutcome> outcomes = await runner.RunAsync(
        request.Resource,
        request.Workers,
        TimeSpan.FromMilliseconds(request.TtlMs),
        TimeSpan.FromMilliseconds(request.WorkMs),
        request.AutoRenew,
        TimeSpan.FromMilliseconds(request.StallFirstWorkerMs),
        CancellationToken.None);

    return Results.Ok(new
    {
        cenario = new
        {
            trabalhadores = request.Workers,
            ttlDoLockMs = request.TtlMs,
            duracaoDoTrabalhoMs = request.WorkMs,
            renovacaoAutomatica = request.AutoRenew,
            leaseSuficiente = request.AutoRenew || request.WorkMs < request.TtlMs
        },

        // Os dois numeros que importam.
        maximoSimultaneoNaSecaoCritica = resource.MaxConcurrentHolders,
        violacoesDeExclusaoMutua = resource.Violations,

        escritasAceitas = outcomes.Count(outcome => outcome.WriteAccepted),
        escritasRejeitadasPeloFencing = outcomes.Count(outcome => outcome.AcquiredLock && !outcome.WriteAccepted),
        trabalhadores = outcomes,
        registro = resource.Log
    });
});

// Aquisicao manual, para inspecionar o lock.
app.MapPost("/lock/acquire", async (AcquireRequest request, DistributedLock distributedLock) =>
{
    LockHandle? handle = await distributedLock.TryAcquireAsync(
        request.Resource,
        TimeSpan.FromMilliseconds(request.TtlMs),
        request.Owner);

    return handle is null
        ? Results.Conflict(new { adquirido = false, motivo = "lock ja esta com outro dono" })
        : Results.Ok(new
        {
            adquirido = true,
            token = handle.Token,
            fencingToken = handle.FencingToken,
            ttlMs = request.TtlMs
        });
});

// Liberacao correta: so libera se o token conferir.
app.MapPost("/lock/release", async (ReleaseRequest request, DistributedLock distributedLock) =>
{
    LockHandle handle = new LockHandle(request.Resource, request.Token, request.Owner, 0, TimeSpan.Zero);
    bool released = await distributedLock.ReleaseAsync(handle);

    return Results.Ok(new
    {
        liberado = released,
        nota = released
            ? "Token conferia: o lock era seu."
            : "Token nao conferia: o lease expirou e o lock e de outro. Nada foi apagado."
    });
});

// Liberacao ingenua: DEL sem conferir dono. Apaga o lock de quem quer que o tenha.
app.MapPost("/lock/release-unsafe", async (AcquireRequest request, DistributedLock distributedLock) =>
{
    bool deleted = await distributedLock.ReleaseUnsafeAsync(request.Resource);

    return Results.Ok(new
    {
        apagado = deleted,
        aviso = "DEL sem conferir o token apaga o lock mesmo que pertenca a outro processo."
    });
});

app.MapGet("/lock/{resource}", async (string resource, DistributedLock distributedLock) =>
    Results.Ok(await distributedLock.GetStateAsync(resource)));

app.MapGet("/resource", (ProtectedResource resource) => Results.Ok(new
{
    maximoSimultaneo = resource.MaxConcurrentHolders,
    violacoes = resource.Violations,
    ultimoFencingAceito = resource.LastAcceptedFence,
    registro = resource.Log
}));

app.Run();

internal sealed record ScenarioRequest(
    string Resource = "relatorio-mensal",
    int Workers = 2,
    int TtlMs = 1000,
    int WorkMs = 3000,
    bool AutoRenew = false,
    int StallFirstWorkerMs = 0);

internal sealed record AcquireRequest(string Resource, string Owner = "manual", int TtlMs = 10000);

internal sealed record ReleaseRequest(string Resource, string Token, string Owner = "manual");
