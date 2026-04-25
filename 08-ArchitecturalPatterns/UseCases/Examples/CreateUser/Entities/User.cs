namespace UseCases.Examples.CreateUser.Entities;

/// <summary>
/// Entidade de domínio User
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public int Age { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private User() { }

    public static User Create(string name, string email, string passwordHash, int age)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            Age = age,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}
