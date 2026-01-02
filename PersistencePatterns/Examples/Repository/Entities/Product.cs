using PersistencePatterns.Core;

namespace PersistencePatterns.Examples.Repository.Entities;

/// <summary>
/// Entidade Product para demonstração do Repository Pattern
/// </summary>
public class Product : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Product() { }

    public static Product Create(string name, string description, decimal price, int stockQuantity)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Preço deve ser positivo");
        Price = newPrice;
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity = quantity;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
