using UseCases.Core;

namespace UseCases.Examples.AuthenticateUser.Entities;

/// <summary>
/// Entidade de domínio: Usuário de autenticação
/// </summary>
public class AuthUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsLocked { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public List<string> Roles { get; private set; } = [];

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public Result RecordFailedLogin()
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedAttempts)
        {
            IsLocked = true;
            LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
            return Result.Failure($"Conta bloqueada por {LockoutDuration.TotalMinutes} minutos após {MaxFailedAttempts} tentativas falhas");
        }

        return Result.Failure($"Credenciais inválidas. Tentativas restantes: {MaxFailedAttempts - FailedLoginAttempts}");
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        IsLocked = false;
        LockoutEnd = null;
        LastLoginAt = DateTime.UtcNow;
    }

    public Result CanLogin()
    {
        if (!IsActive)
            return Result.Failure("Conta desativada");

        if (IsLocked && LockoutEnd > DateTime.UtcNow)
            return Result.Failure($"Conta bloqueada até {LockoutEnd:HH:mm}");

        // Auto-unlock após período de lockout
        if (IsLocked && LockoutEnd <= DateTime.UtcNow)
        {
            IsLocked = false;
            LockoutEnd = null;
            FailedLoginAttempts = 0;
        }

        return Result.Success();
    }
}
