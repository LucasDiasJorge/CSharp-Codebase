namespace UseCases.Examples.CreateUser.DTOs;

/// <summary>
/// DTO de entrada para criação de usuário
/// </summary>
public record CreateUserInput(
    string Name,
    string Email,
    string Password,
    int Age
);
