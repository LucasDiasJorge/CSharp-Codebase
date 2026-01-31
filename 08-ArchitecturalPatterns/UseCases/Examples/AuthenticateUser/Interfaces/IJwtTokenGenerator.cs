namespace UseCases.Examples.AuthenticateUser.Interfaces;

/// <summary>
/// Gerador de tokens JWT
/// </summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
}
