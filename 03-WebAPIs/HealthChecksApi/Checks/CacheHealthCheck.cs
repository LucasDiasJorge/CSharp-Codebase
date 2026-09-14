using HealthChecksApi.Dependencies;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksApi.Checks;

/// <summary>
/// Dependência não crítica. Sem cache a aplicação fica mais lenta, mas continua
/// atendendo — por isso o registro usa <c>failureStatus: Degraded</c>, e o resultado
/// degradado não tira a instância do balanceador.
/// </summary>
public sealed class CacheHealthCheck : IHealthCheck
{
    private static readonly TimeSpan LatencyBudget = TimeSpan.FromMilliseconds(500);


    private readonly DependencySimulator _dependencies;

    public CacheHealthCheck(DependencySimulator dependencies)
    {
        _dependencies = dependencies;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            TimeSpan latency = await _dependencies.ProbeAsync(DependencySimulator.Cache, cancellationToken).ConfigureAwait(false);

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["latencyMs"] = latency.TotalMilliseconds,
                ["budgetMs"] = LatencyBudget.TotalMilliseconds
            };

            // Responder devagar tambem e sintoma. Um check que so olha "respondeu ou
            // nao" deixa passar a degradacao que antecede a queda.
            if (latency > LatencyBudget)
            {
                return HealthCheckResult.Degraded("Cache respondendo acima do tempo aceitavel.", data: data);
            }

            return HealthCheckResult.Healthy("Cache respondendo.", data);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "A checagem do cache excedeu o tempo limite.");
        }
        catch (InvalidOperationException ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex);
        }
    }
}
