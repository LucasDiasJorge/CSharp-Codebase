using GoodOrderApi.Domain.ValueObjects;

namespace GoodOrderApi.Domain.Entities;

// ✅ REGRA 8: Máximo duas variáveis de instância (agrupamos em objetos)
// ✅ REGRA 9: Expor comportamento em vez de getters/setters

/// <summary>
/// Entidade que representa um item do pedido.
/// </summary>
public class OrderItem
{
    public int Id { get; }
    public OrderItemDetails Details { get; }

    private OrderItem(int id, OrderItemDetails details)
    {
        Id = id;
        Details = details;
    }

    public static OrderItem Create(int id, Product product, Quantity quantity)
    {
        var details = OrderItemDetails.Create(product, quantity);
        return new OrderItem(id, details);
    }

    public Money CalculateTotal() => Details.CalculateTotal();
    
    public Quantity GetQuantity() => Details.Quantity;
}

/// <summary>
/// Value Object com detalhes do item do pedido.
/// ✅ REGRA 8: Apenas duas variáveis de instância.
/// </summary>
public sealed class OrderItemDetails
{
    public ProductSnapshot Product { get; }
    public Quantity Quantity { get; }

    private OrderItemDetails(ProductSnapshot product, Quantity quantity)
    {
        Product = product;
        Quantity = quantity;
    }

    public static OrderItemDetails Create(Product product, Quantity quantity)
    {
        var snapshot = ProductSnapshot.FromProduct(product);
        return new OrderItemDetails(snapshot, quantity);
    }

    public Money CalculateTotal() => Product.Price.MultiplyBy(Quantity.Value);
}

/// <summary>
/// Value Object que captura o estado do produto no momento da compra.
/// ✅ REGRA 8: Apenas duas variáveis de instância.
/// </summary>
public sealed class ProductSnapshot
{
    public string Name { get; }
    public Money Price { get; }

    private ProductSnapshot(string name, Money price)
    {
        Name = name;
        Price = price;
    }

    public static ProductSnapshot FromProduct(Product product)
    {
        return new ProductSnapshot(
            product.Info.Identification.Name,
            product.Info.Pricing.Price
        );
    }
}
