namespace UseCases.Examples.AuthenticateUser.Entities;

/// <summary>
/// Entidade: Refresh Token
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsRevoked { get; private set; }

    public static RefreshToken Create(Guid userId, string? ipAddress, string? userAgent, int expirationDays = 7)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsRevoked = false
        };
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public void Revoke() => IsRevoked = true;

    public bool IsValid() => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}
