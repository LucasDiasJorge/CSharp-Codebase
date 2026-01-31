namespace UseCases.Examples.AuthenticateUser.Entities;

/// <summary>
/// Entidade: Log de auditoria de login
/// </summary>
public class LoginAuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }

    public static LoginAuditLog Create(string email, Guid? userId, bool success, string? failureReason, string? ipAddress, string? userAgent)
    {
        return new LoginAuditLog
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserId = userId,
            Success = success,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };
    }
}
