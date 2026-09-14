using HealthChecksApi.Checks;
using HealthChecksApi.Dependencies;
using HealthChecksApi.Reporting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DependencySimulator>();

builder.Services.AddHealthChecks()

    // liveness: so responde "o processo esta vivo?". Nenhuma dependencia externa aqui.
    // Incluir banco nesta sonda transforma uma queda do banco em reinicio de todas as
    // instancias — troca uma indisponibilidade parcial por uma total.
    .AddCheck("self", () => HealthCheckResult.Healthy("Processo respondendo."), tags: ["live"])

    // readiness: pode esta instancia receber trafego agora?
    .AddCheck<DatabaseHealthCheck>(
        "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"],
        timeout: TimeSpan.FromSeconds(2))

    .AddCheck<MessageBrokerHealthCheck>(
        "broker",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"],
        timeout: TimeSpan.FromSeconds(2))

    // Dependencia nao critica: a falha dela degrada, nao derruba.
    .AddCheck<CacheHealthCheck>(
        "cache",
        failureStatus: HealthStatus.Degraded,
        tags: ["ready"],
        timeout: TimeSpan.FromSeconds(2));

WebApplication app = builder.Build();

// Sonda de liveness. Se falhar, o orquestrador reinicia o container.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = HealthReportWriter.WriteAsync
});

// Sonda de readiness. Se falhar, o orquestrador tira a instancia do balanceador, mas
// nao a reinicia — ela pode voltar sozinha quando a dependencia voltar.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthReportWriter.WriteAsync,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,

        // Degraded continua recebendo trafego de proposito: perder o cache deixa a
        // aplicacao lenta, mas tirar todas as instancias do ar a deixa indisponivel.
        [HealthStatus.Degraded] = StatusCodes.Status200OK,

        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

// Visao completa, para humano e para painel — nao para sonda de orquestrador.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthReportWriter.WriteAsync
});

// Endpoints de simulacao: viram o estado das dependencias para que os cenarios de
// falha sejam observaveis sem derrubar servico nenhum.
app.MapPost("/simulation/{dependency}", (string dependency, DependencyState state, DependencySimulator simulator) =>
{
    return simulator.TrySetState(dependency, state)
        ? Results.Ok(new { dependency, state = state.ToString() })
        : Results.NotFound(new { dependency, conhecidas = simulator.States.Keys });
});

app.MapGet("/simulation", (DependencySimulator simulator) =>
{
    Dictionary<string, string> snapshot = new Dictionary<string, string>();
    foreach (KeyValuePair<string, DependencyState> pair in simulator.States)
    {
        snapshot[pair.Key] = pair.Value.ToString();
    }

    return Results.Ok(snapshot);
});

app.Run();
