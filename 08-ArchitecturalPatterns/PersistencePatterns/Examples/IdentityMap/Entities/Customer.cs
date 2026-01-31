using PersistencePatterns.Core;

namespace PersistencePatterns.Examples.IdentityMap.Entities;

/// <summary>
/// Entidade Customer para demonstração do Identity Map
/// </summary>
public class Customer : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Customer() { }

    public static Customer Create(Guid id, string name, string email)
    {
        return new Customer
        {
            Id = id,
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateName(string name) => Name = name;
}
