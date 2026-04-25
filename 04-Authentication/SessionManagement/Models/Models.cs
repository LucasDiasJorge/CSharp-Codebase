namespace SessionManagement.Models;

// Stored in EF Core (InMemory) — users only
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Stored in Redis as JSON — not an EF entity
public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }

    /// <summary>SHA-256 hash of the refresh token (never stored in plain text).</summary>
    public string RefreshTokenHash { get; set; } = string.Empty;

    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
}
