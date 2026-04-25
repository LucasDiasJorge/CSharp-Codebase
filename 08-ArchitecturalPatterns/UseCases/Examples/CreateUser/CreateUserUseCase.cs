using UseCases.Core;
using UseCases.Examples.CreateUser.DTOs;
using UseCases.Examples.CreateUser.Entities;
using UseCases.Examples.CreateUser.Interfaces;

namespace UseCases.Examples.CreateUser;

/// <summary>
/// Use Case: Criar novo usuário
/// 
/// Responsabilidades:
/// - Validar dados de entrada
/// - Verificar se email já existe
/// - Criar hash da senha
/// - Persistir usuário
/// - Enviar email de boas-vindas
/// </summary>
public class CreateUserUseCase : IUseCase<CreateUserInput, Result<CreateUserOutput>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly INotificationService _notificationService;

    public CreateUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _notificationService = notificationService;
    }

    public async Task<Result<CreateUserOutput>> ExecuteAsync(
        CreateUserInput input,
        CancellationToken cancellationToken = default)
    {
        // 1. Validações de entrada
        var validationResult = Validate(input);
        if (validationResult.IsFailure)
            return Result<CreateUserOutput>.Failure(validationResult.Errors);

        // 2. Verificar se email já existe
        if (await _userRepository.ExistsAsync(input.Email, cancellationToken))
            return Result<CreateUserOutput>.Failure("Email já está em uso");

        // 3. Criar hash da senha
        var passwordHash = _passwordHasher.Hash(input.Password);

        // 4. Criar entidade de domínio
        var user = User.Create(input.Name, input.Email, passwordHash, input.Age);

        // 5. Persistir no banco
        await _userRepository.AddAsync(user, cancellationToken);

        // 6. Enviar notificação (não bloqueia o fluxo principal)
        _ = _notificationService.SendWelcomeEmailAsync(user.Email, user.Name, cancellationToken);

        // 7. Retornar resultado
        return Result<CreateUserOutput>.Success(new CreateUserOutput(
            user.Id,
            user.Name,
            user.Email,
            user.CreatedAt
        ));
    }

    private static Result Validate(CreateUserInput input)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Name))
            errors.Add("Nome é obrigatório");
        else if (input.Name.Length < 2)
            errors.Add("Nome deve ter pelo menos 2 caracteres");

        if (string.IsNullOrWhiteSpace(input.Email))
            errors.Add("Email é obrigatório");
        else if (!input.Email.Contains('@'))
            errors.Add("Email inválido");

        if (string.IsNullOrWhiteSpace(input.Password))
            errors.Add("Senha é obrigatória");
        else if (input.Password.Length < 6)
            errors.Add("Senha deve ter pelo menos 6 caracteres");

        if (input.Age < 18)
            errors.Add("Usuário deve ter pelo menos 18 anos");

        return errors.Count > 0 ? Result.Failure(errors) : Result.Success();
    }
}
