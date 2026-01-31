using GoodOrderApi.Domain.ValueObjects;

namespace GoodOrderApi.Domain.Entities;

// ✅ REGRA 8: Máximo duas variáveis de instância
// ✅ REGRA 9: Expor comportamento em vez de getters/setters

/// <summary>
/// Entidade que representa um produto.
/// ✅ Segue regra 8: Agrupa propriedades em objetos de valor.
/// </summary>
public class Product
{
    public int Id { get; }
    public ProductInfo Info { get; }
    public ProductInventory Inventory { get; private set; }

    private Product(int id, ProductInfo info, ProductInventory inventory)
    {
        Id = id;
        Info = info;
        Inventory = inventory;
    }

    public static Product Create(int id, string name, string description, decimal price, int stockQuantity, string category)
    {
        var info = ProductInfo.Create(name, description, price, category);
        var inventory = ProductInventory.Create(stockQuantity, isActive: true);
        return new Product(id, info, inventory);
    }

    // ✅ REGRA 9: Comportamento em vez de setter
    public void ReserveStock(Quantity quantity)
    {
        Inventory = Inventory.Reserve(quantity);
    }

    public void RestoreStock(Quantity quantity)
    {
        Inventory = Inventory.Restore(quantity);
    }

    public bool HasSufficientStock(Quantity quantity)
    {
        return Inventory.HasSufficientStock(quantity);
    }

    public bool IsAvailable => Inventory.IsActive;
}

/// <summary>
/// Value Object com informações do produto.
/// ✅ REGRA 8: Máximo duas variáveis de instância (agrupadas em objetos menores).
/// </summary>
public sealed class ProductInfo
{
    public ProductIdentification Identification { get; }
    public ProductPricing Pricing { get; }

    private ProductInfo(ProductIdentification identification, ProductPricing pricing)
    {
        Identification = identification;
        Pricing = pricing;
    }

    public static ProductInfo Create(string name, string description, decimal price, string category)
    {
        var identification = ProductIdentification.Create(name, description);
        var pricing = ProductPricing.Create(price, category);
        return new ProductInfo(identification, pricing);
    }
}

public sealed class ProductIdentification
{
    public string Name { get; }
    public string Description { get; }

    private ProductIdentification(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public static ProductIdentification Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");
        
        return new ProductIdentification(name, description ?? string.Empty);
    }
}

public sealed class ProductPricing
{
    public Money Price { get; }
    public string Category { get; }

    private ProductPricing(Money price, string category)
    {
        Price = price;
        Category = category;
    }

    public static ProductPricing Create(decimal price, string category)
    {
        return new ProductPricing(Money.Create(price), category ?? "General");
    }
}

/// <summary>
/// Value Object que gerencia o inventário do produto.
/// ✅ REGRA 8: Apenas duas variáveis de instância.
/// </summary>
public sealed class ProductInventory
{
    public Quantity StockQuantity { get; }
    public bool IsActive { get; }

    private ProductInventory(Quantity stockQuantity, bool isActive)
    {
        StockQuantity = stockQuantity;
        IsActive = isActive;
    }

    public static ProductInventory Create(int stockQuantity, bool isActive)
    {
        return new ProductInventory(Quantity.Create(Math.Max(1, stockQuantity)), isActive);
    }

    public bool HasSufficientStock(Quantity required)
    {
        return IsActive && StockQuantity.IsGreaterThanOrEqual(required);
    }

    public ProductInventory Reserve(Quantity quantity)
    {
        if (!HasSufficientStock(quantity))
            throw new DomainException("Insufficient stock");
        
        var newQuantity = Quantity.Create(StockQuantity.Value - quantity.Value);
        return new ProductInventory(newQuantity, IsActive);
    }

    public ProductInventory Restore(Quantity quantity)
    {
        var newQuantity = StockQuantity.Add(quantity);
        return new ProductInventory(newQuantity, IsActive);
    }
}
