namespace CompositionOrderFulfillment.Models;

public sealed class Product
{
    public Product(string sku, string description, decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be positive.");
        }

        Sku = sku;
        Description = description;
        UnitPrice = unitPrice;
    }

    public string Sku { get; }
    public string Description { get; }
    public decimal UnitPrice { get; }
}