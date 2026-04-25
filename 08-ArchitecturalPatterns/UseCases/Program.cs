using UseCases.Core;
using UseCases.Examples.CreateUser;
using UseCases.Examples.CreateUser.DTOs;
using UseCases.Examples.CreateUser.Entities;
using UseCases.Examples.CreateUser.Interfaces;

namespace UseCases;

/// <summary>
/// Programa de demonstração dos Use Cases
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           USE CASES - EXEMPLOS EM C#                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Demonstração do padrão Result
        DemonstrateResultPattern();

        // Demonstração de validação
        await DemonstrateValidationAsync();

        Console.WriteLine("\n✅ Todos os exemplos executados com sucesso!");
        Console.WriteLine("\nConsulte a documentação de cada Use Case para mais detalhes:");
        Console.WriteLine("  📁 Examples/CreateUser/README.md");
        Console.WriteLine("  📁 Examples/TransferMoney/README.md");
        Console.WriteLine("  📁 Examples/ProcessOrder/README.md");
        Console.WriteLine("  📁 Examples/AuthenticateUser/README.md");
    }

    static void DemonstrateResultPattern()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("📦 PADRÃO RESULT - Tratamento de Erros sem Exceções");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        // Sucesso
        var successResult = Result<int>.Success(42);
        Console.WriteLine($"Resultado de Sucesso:");
        Console.WriteLine($"  IsSuccess: {successResult.IsSuccess}");
        Console.WriteLine($"  Value: {successResult.Value}");

        // Falha
        var failureResult = Result<int>.Failure("Operação não permitida");
        Console.WriteLine($"\nResultado de Falha:");
        Console.WriteLine($"  IsSuccess: {failureResult.IsSuccess}");
        Console.WriteLine($"  Error: {failureResult.Error}");

        // Múltiplos erros
        var multipleErrors = Result.Failure(new[] { "Erro 1", "Erro 2", "Erro 3" });
        Console.WriteLine($"\nMúltiplos Erros:");
        foreach (var error in multipleErrors.Errors)
        {
            Console.WriteLine($"  - {error}");
        }

        Console.WriteLine();
    }

    static async Task DemonstrateValidationAsync()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("✅ VALIDAÇÃO - Exemplo com CreateUserUseCase");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        // Usando implementações fake para demonstração
        var userRepository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var notificationService = new FakeNotificationService();

        var useCase = new CreateUserUseCase(userRepository, passwordHasher, notificationService);

        // Caso 1: Dados inválidos
        Console.WriteLine("Caso 1: Tentando criar usuário com dados inválidos");
        var invalidInput = new CreateUserInput("", "email-invalido", "123", 15);
        var invalidResult = await useCase.ExecuteAsync(invalidInput);

        Console.WriteLine($"  Sucesso: {invalidResult.IsSuccess}");
        Console.WriteLine("  Erros:");
        foreach (var error in invalidResult.Errors)
        {
            Console.WriteLine($"    ❌ {error}");
        }

        // Caso 2: Dados válidos
        Console.WriteLine("\nCaso 2: Criando usuário com dados válidos");
        var validInput = new CreateUserInput("João Silva", "joao@email.com", "senha123", 25);
        var validResult = await useCase.ExecuteAsync(validInput);

        Console.WriteLine($"  Sucesso: {validResult.IsSuccess}");
        if (validResult.IsSuccess)
        {
            Console.WriteLine($"  ✅ ID: {validResult.Value!.Id}");
            Console.WriteLine($"  ✅ Nome: {validResult.Value.Name}");
            Console.WriteLine($"  ✅ Email: {validResult.Value.Email}");
            Console.WriteLine($"  ✅ Criado em: {validResult.Value.CreatedAt}");
        }

        // Caso 3: Email duplicado
        Console.WriteLine("\nCaso 3: Tentando criar usuário com email duplicado");
        var duplicateResult = await useCase.ExecuteAsync(validInput);

        Console.WriteLine($"  Sucesso: {duplicateResult.IsSuccess}");
        Console.WriteLine($"  ❌ Erro: {duplicateResult.Error}");

        Console.WriteLine();
    }
}

#region Fake Implementations for Demo

internal class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(user);
    }

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Any(u => u.Email == email));
    }
}

internal class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed_{password}";
    public bool Verify(string password, string hash) => hash == $"hashed_{password}";
}

internal class FakeNotificationService : INotificationService
{
    public Task SendWelcomeEmailAsync(string email, string name, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"  📧 Email de boas-vindas enviado para {email}");
        return Task.CompletedTask;
    }
}
