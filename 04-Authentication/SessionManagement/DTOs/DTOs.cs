namespace SessionManagement.DTOs;

public record RegisterRequest(
    string Username,
    string Email,
    string Password);

public record LoginRequest(
    string Username,
    string Password,
    string? DeviceName);

public record RefreshRequest(
    string RefreshToken);

public record RevokeSessionRequest(
    string? Reason);

public record AuthResponse(
    Guid SessionId,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    DateTime SessionExpiresAt);

public record SessionInfo(
    Guid SessionId,
    string? DeviceName,
    string? IPAddress,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime LastActivityAt,
    DateTime ExpiresAt,
    bool IsCurrent);
