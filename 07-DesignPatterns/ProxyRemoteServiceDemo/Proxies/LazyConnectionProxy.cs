using Microsoft.Extensions.Logging;
using ProxyRemoteServiceDemo.Contracts;

namespace ProxyRemoteServiceDemo.Proxies;

/// <summary>
/// Proxy virtual: adia a criação do serviço real até a primeira chamada de verdade.
/// Se o cliente nunca usar o serviço, a conexão nunca é aberta e o custo nunca é pago.
/// </summary>
public sealed class LazyConnectionProxy : IReportService
{
    private readonly Func<IReportService> _factory;
    private readonly ILogger<LazyConnectionProxy> _logger;
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

    private IReportService? _real;

    public LazyConnectionProxy(Func<IReportService> factory, ILogger<LazyConnectionProxy> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public bool IsConnected => _real is not null;

    public async Task<string> GenerateAsync(string reportName, CancellationToken cancellationToken)
    {
        IReportService real = await EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        return await real.GenerateAsync(reportName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> CountAvailableAsync(CancellationToken cancellationToken)
    {
        IReportService real = await EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        return await real.CountAvailableAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReportService> EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        if (_real is not null)
        {
            return _real;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Segunda checagem dentro do semaforo: sem ela, duas chamadas simultaneas
            // criariam duas conexoes — e o custo que o proxy existe para evitar seria
            // pago em dobro.
            if (_real is not null)
            {
                return _real;
            }

            _logger.LogInformation("Primeira chamada: criando o servico real agora.");
            _real = _factory();

            return _real;
        }
        finally
        {
            _gate.Release();
        }
    }
}
