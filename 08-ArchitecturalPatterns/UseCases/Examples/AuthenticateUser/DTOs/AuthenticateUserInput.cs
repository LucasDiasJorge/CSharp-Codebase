namespace UseCases.Examples.AuthenticateUser.DTOs;

/// <summary>
/// DTO de entrada para autenticação
/// </summary>
public record AuthenticateUserInput(
    string Email,
    string Password,
    string? IpAddress = null,
    string? UserAgent = null
);
