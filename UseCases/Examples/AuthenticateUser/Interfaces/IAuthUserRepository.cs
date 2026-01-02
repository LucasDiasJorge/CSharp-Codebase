using UseCases.Examples.AuthenticateUser.Entities;

namespace UseCases.Examples.AuthenticateUser.Interfaces;

/// <summary>
/// Repositório de usuários de autenticação
/// </summary>
public interface IAuthUserRepository
{
    Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateAsync(AuthUser user, CancellationToken cancellationToken = default);
}
