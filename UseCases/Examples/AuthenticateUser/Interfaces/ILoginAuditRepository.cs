using UseCases.Examples.AuthenticateUser.Entities;

namespace UseCases.Examples.AuthenticateUser.Interfaces;

/// <summary>
/// Repositório de logs de auditoria de login
/// </summary>
public interface ILoginAuditRepository
{
    Task AddAsync(LoginAuditLog log, CancellationToken cancellationToken = default);
}
