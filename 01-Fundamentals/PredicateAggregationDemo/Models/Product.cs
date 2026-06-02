namespace PredicateAggregationDemo.Models;

public sealed class Product
{
    public Product(
        string name,
        string description,
        string category,
        decimal price,
        bool isActive)
    {
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        IsActive = isActive;
    }

    public string Name { get; }

    public string Description { get; }

    public string Category { get; }

    public decimal Price { get; }

    public bool IsActive { get; }
}
