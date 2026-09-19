using Microsoft.Extensions.Logging;
using ProxyRemoteServiceDemo.Contracts;

namespace ProxyRemoteServiceDemo.Proxies;

/// <summary>
/// Proxy de proteção: decide se a chamada pode acontecer antes de repassá-la. A regra
/// de acesso fica aqui, fora do serviço real e fora do cliente — os dois continuam sem
/// saber que ela existe.
/// </summary>
public sealed class AuthorizationProxy : IReportService
{
    public const string GeneratePermission = "reports:generate";
    public const string ReadPermission = "reports:read";

    private readonly IReportService _inner;
    private readonly CallerContext _caller;
    private readonly ILogger<AuthorizationProxy> _logger;

    public AuthorizationProxy(IReportService inner, CallerContext caller, ILogger<AuthorizationProxy> logger)
    {
        _inner = inner;
        _caller = caller;
        _logger = logger;
    }

    public Task<string> GenerateAsync(string reportName, CancellationToken cancellationToken)
    {
        EnsurePermission(GeneratePermission);

        return _inner.GenerateAsync(reportName, cancellationToken);
    }

    public Task<int> CountAvailableAsync(CancellationToken cancellationToken)
    {
        EnsurePermission(ReadPermission);

        return _inner.CountAvailableAsync(cancellationToken);
    }

    private void EnsurePermission(string permission)
    {
        if (_caller.Has(permission))
        {
            return;
        }

        _logger.LogWarning("{Usuario} sem a permissao {Permissao}: chamada bloqueada antes de sair.", _caller.User, permission);

        // Bloqueia ANTES de repassar: o servico real nem e criado, nem e chamado.
        throw new UnauthorizedAccessException($"{_caller.User} nao tem a permissao {permission}.");
    }
}
