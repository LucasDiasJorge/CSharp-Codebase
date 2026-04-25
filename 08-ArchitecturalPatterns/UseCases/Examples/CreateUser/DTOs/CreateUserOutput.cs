namespace UseCases.Examples.CreateUser.DTOs;

/// <summary>
/// DTO de saída após criação de usuário
/// </summary>
public record CreateUserOutput(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt
);
