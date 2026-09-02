using GenericConstraintsDemo.Contracts;

namespace GenericConstraintsDemo.Models;

public sealed class Product : IEntity, IPricedItem
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
