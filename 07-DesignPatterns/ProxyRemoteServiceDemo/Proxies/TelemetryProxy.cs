using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProxyRemoteServiceDemo.Contracts;

namespace ProxyRemoteServiceDemo.Proxies;

/// <summary>
/// Proxy de telemetria: mede e registra cada chamada sem que o serviço real nem o
/// cliente saibam. É o exemplo mais claro de por que o proxy precisa implementar a
/// mesma interface — nada no cliente muda para ganhar instrumentação.
/// </summary>
public sealed class TelemetryProxy : IReportService
{
    private readonly IReportService _inner;
    private readonly ILogger<TelemetryProxy> _logger;
    private readonly List<CallRecord> _records = new List<CallRecord>();

    public TelemetryProxy(IReportService inner, ILogger<TelemetryProxy> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public IReadOnlyList<CallRecord> Records => _records;

    public Task<string> GenerateAsync(string reportName, CancellationToken cancellationToken)
    {
        return MeasureAsync(nameof(GenerateAsync), () => _inner.GenerateAsync(reportName, cancellationToken));
    }

    public Task<int> CountAvailableAsync(CancellationToken cancellationToken)
    {
        return MeasureAsync(nameof(CountAvailableAsync), () => _inner.CountAvailableAsync(cancellationToken));
    }

    private async Task<T> MeasureAsync<T>(string operation, Func<Task<T>> call)
    {
        Stopwatch watch = Stopwatch.StartNew();
        bool succeeded = false;

        try
        {
            T result = await call().ConfigureAwait(false);
            succeeded = true;

            return result;
        }
        finally
        {
            // finally, nao apenas no caminho feliz: chamada que falha e justamente a
            // que mais interessa medir.
            watch.Stop();
            _records.Add(new CallRecord(operation, watch.ElapsedMilliseconds, succeeded));

            _logger.LogInformation(
                "{Operacao} levou {Tempo}ms e {Resultado}.",
                operation,
                watch.ElapsedMilliseconds,
                succeeded ? "concluiu" : "falhou");
        }
    }
}

public sealed record CallRecord(string Operation, long ElapsedMs, bool Succeeded);
