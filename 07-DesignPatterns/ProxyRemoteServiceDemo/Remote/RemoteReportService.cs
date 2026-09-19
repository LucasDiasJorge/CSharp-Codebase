using Microsoft.Extensions.Logging;
using ProxyRemoteServiceDemo.Contracts;

namespace ProxyRemoteServiceDemo.Remote;

/// <summary>
/// O serviço real (real subject). Caro de criar — simula abrir conexão e autenticar —
/// e lento a cada chamada. As duas características são o que justificam um proxy.
///
/// Note que ele não sabe nada sobre autorização, métricas ou carregamento tardio.
/// Toda essa responsabilidade fica nos proxies, e é por isso que ela pode mudar sem
/// que este arquivo seja tocado.
/// </summary>
public sealed class RemoteReportService : IReportService
{
    /// <summary>Custo de estabelecer a conexão, pago uma vez na construção.</summary>
    public static readonly TimeSpan ConnectionCost = TimeSpan.FromMilliseconds(600);

    private static readonly TimeSpan CallCost = TimeSpan.FromMilliseconds(150);

    private static int _instancesCreated;

    private readonly ILogger _logger;

    public RemoteReportService(ILogger logger)
    {
        _logger = logger;

        Interlocked.Increment(ref _instancesCreated);

        // Construtor caro de proposito: e o custo que o proxy virtual evita quando o
        // servico nunca chega a ser usado.
        Thread.Sleep(ConnectionCost);

        _logger.LogWarning("Conexao remota estabelecida ({Custo}ms). Instancias criadas ate agora: {Total}.", ConnectionCost.TotalMilliseconds, _instancesCreated);
    }

    public static int InstancesCreated => Volatile.Read(ref _instancesCreated);

    public static void ResetCounter() => Interlocked.Exchange(ref _instancesCreated, 0);

    public async Task<string> GenerateAsync(string reportName, CancellationToken cancellationToken)
    {
        await Task.Delay(CallCost, cancellationToken).ConfigureAwait(false);

        return $"[relatorio {reportName} gerado em {DateTimeOffset.UtcNow:HH:mm:ss.fff}]";
    }

    public async Task<int> CountAvailableAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(CallCost, cancellationToken).ConfigureAwait(false);

        return 7;
    }
}
