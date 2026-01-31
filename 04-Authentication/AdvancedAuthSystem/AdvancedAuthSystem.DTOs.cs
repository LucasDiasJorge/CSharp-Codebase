namespace AdvancedAuthSystem.DTOs.Auth;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Department,
    DateTime DateOfBirth
);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    bool RequiresTwoFactor = false,
    string? TempToken = null
);

public record RefreshTokenRequest(string RefreshToken);

public record TwoFactorRequest(string Username, string Code, string TempToken);

public record Enable2FAResponse(string Secret, string QrCodeUrl, string ManualEntryKey);

namespace AdvancedAuthSystem.DTOs.User;

public record UserDto(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? Department,
    DateTime DateOfBirth,
    bool TwoFactorEnabled,
    List<string> Roles
);

public record UpdateUserDto(
    string? FirstName,
    string? LastName,
    string? Department
);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

namespace AdvancedAuthSystem.DTOs.Resource;

public record CreateResourceRequest(
    string Name,
    string Description,
    string? Department,
    bool IsPublic
);

public record UpdateResourceRequest(
    string? Name,
    string? Description,
    bool? IsPublic
);

public record ResourceDto(
    int Id,
    string Name,
    string Description,
    int OwnerId,
    string OwnerUsername,
    string? Department,
    DateTime CreatedAt,
    bool IsPublic
);
