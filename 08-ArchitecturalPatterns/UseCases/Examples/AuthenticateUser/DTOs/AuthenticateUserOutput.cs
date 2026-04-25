namespace UseCases.Examples.AuthenticateUser.DTOs;

/// <summary>
/// DTO de saída da autenticação
/// </summary>
public record AuthenticateUserOutput(
    Guid UserId,
    string Email,
    string Name,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    IEnumerable<string> Roles
);
