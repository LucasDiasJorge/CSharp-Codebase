using System.ComponentModel.DataAnnotations;

namespace RefreshTokenRotationDemo.Models;

public sealed class LoginRequest
{
    [Required]
    public string? Username { get; set; }

    [Required]
    public string? Password { get; set; }
}

public sealed class RefreshRequest
{
    [Required]
    public string? RefreshToken { get; set; }
}

/// <summary>
/// Par devolvido a cada login ou rotação. O refresh token aparece aqui uma única vez —
/// depois disso, só existe o hash no servidor.
/// </summary>
public sealed class TokenResponse
{
    public TokenResponse(string accessToken, int accessTokenExpiresInSeconds, string refreshToken, DateTimeOffset refreshTokenExpiresAt, string familyId)
    {
        AccessToken = accessToken;
        AccessTokenExpiresInSeconds = accessTokenExpiresInSeconds;
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        FamilyId = familyId;
    }

    public string AccessToken { get; }

    public int AccessTokenExpiresInSeconds { get; }

    public string RefreshToken { get; }

    public DateTimeOffset RefreshTokenExpiresAt { get; }

    /// <summary>Exposto só para tornar a linhagem visível no exemplo.</summary>
    public string FamilyId { get; }
}
