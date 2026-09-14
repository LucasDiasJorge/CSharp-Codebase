using HealthChecksApi.Dependencies;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksApi.Checks;

/// <summary>
/// Dependência crítica: sem banco, a aplicação não consegue atender requisição alguma.
/// Marcada com a tag <c>ready</c> — nunca com <c>live</c>, ou uma queda do banco faria
/// o orquestrador reiniciar instâncias perfeitamente saudáveis.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly DependencySimulator _dependencies;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(DependencySimulator dependencies, ILogger<DatabaseHealthCheck> logger)
    {
        _dependencies = dependencies;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            TimeSpan latency = await _dependencies.ProbeAsync(DependencySimulator.Database, cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy(
                "Conexao com o banco estabelecida.",
                new Dictionary<string, object> { ["latencyMs"] = latency.TotalMilliseconds });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // O timeout configurado no AddCheck cancela o token. Sem este catch, a
            // excecao subiria como falha generica e a mensagem perderia a causa.
            _logger.LogWarning("Checagem do banco excedeu o tempo limite.");

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                "A checagem do banco excedeu o tempo limite.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Banco indisponivel.");

            return new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex);
        }
    }
}
