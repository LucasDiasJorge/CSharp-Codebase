using HealthChecksApi.Dependencies;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksApi.Checks;

/// <summary>
/// Outra dependência crítica para readiness. Usa <c>context.Registration.FailureStatus</c>
/// em vez de <c>HealthCheckResult.Unhealthy()</c> fixo: assim o registro decide o peso
/// da falha, e a mesma classe serve para dependência crítica ou opcional.
/// </summary>
public sealed class MessageBrokerHealthCheck : IHealthCheck
{
    private readonly DependencySimulator _dependencies;

    public MessageBrokerHealthCheck(DependencySimulator dependencies)
    {
        _dependencies = dependencies;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            TimeSpan latency = await _dependencies.ProbeAsync(DependencySimulator.MessageBroker, cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy(
                "Broker respondendo.",
                new Dictionary<string, object> { ["latencyMs"] = latency.TotalMilliseconds });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "A checagem do broker excedeu o tempo limite.");
        }
        catch (InvalidOperationException ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex);
        }
    }
}
