using UseCases.Examples.AuthenticateUser.Entities;

namespace UseCases.Examples.AuthenticateUser.Interfaces;

/// <summary>
/// Repositório de refresh tokens
/// </summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
