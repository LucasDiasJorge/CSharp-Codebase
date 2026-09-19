namespace ProxyRemoteServiceDemo.Contracts;

/// <summary>
/// O contrato que o cliente conhece. Tudo no padrão Proxy depende disto: o proxy
/// implementa a MESMA interface do serviço real, então o cliente não tem como (e não
/// precisa) distinguir um do outro.
/// </summary>
public interface IReportService
{
    Task<string> GenerateAsync(string reportName, CancellationToken cancellationToken);

    Task<int> CountAvailableAsync(CancellationToken cancellationToken);
}
