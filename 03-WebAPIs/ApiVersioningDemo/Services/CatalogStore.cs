using ApiVersioningDemo.Models;

namespace ApiVersioningDemo.Services;

/// <summary>
/// Fonte de dados em memória. Existe uma única representação interna; cada versão da
/// API projeta essa representação no seu próprio contrato. É essa separação que torna
/// o versionamento sustentável — o domínio não é versionado, o contrato é.
/// </summary>
public sealed class CatalogStore
{
    private readonly IReadOnlyList<CatalogItem> _items =
    [
        new CatalogItem(1, "Teclado mecanico", 349.90m, "BRL", ["periferico", "entrada"], "Ana Souza", "confirmado"),
        new CatalogItem(2, "Monitor 27 polegadas", 1899.00m, "BRL", ["video", "escritorio"], "Bruno Lima", "em-separacao"),
        new CatalogItem(3, "Cadeira ergonomica", 2450.50m, "BRL", ["mobiliario"], "Carla Dias", "entregue")
    ];

    public IReadOnlyList<ProductV1> GetProductsV1()
    {
        List<ProductV1> products = new List<ProductV1>(_items.Count);
        foreach (CatalogItem item in _items)
        {
            products.Add(new ProductV1(item.Id, item.Name, item.Amount));
        }

        return products;
    }

    public IReadOnlyList<ProductV2> GetProductsV2()
    {
        List<ProductV2> products = new List<ProductV2>(_items.Count);
        foreach (CatalogItem item in _items)
        {
            products.Add(new ProductV2(item.Id, item.Name, new MoneyV2(item.Amount, item.Currency), item.Tags));
        }

        return products;
    }

    public IReadOnlyList<OrderV1> GetOrdersV1()
    {
        List<OrderV1> orders = new List<OrderV1>(_items.Count);
        foreach (CatalogItem item in _items)
        {
            orders.Add(new OrderV1(item.Id, item.Customer, item.Amount));
        }

        return orders;
    }

    public IReadOnlyList<OrderV2> GetOrdersV2()
    {
        List<OrderV2> orders = new List<OrderV2>(_items.Count);
        foreach (CatalogItem item in _items)
        {
            orders.Add(new OrderV2(item.Id, item.Customer, item.Amount, item.Status));
        }

        return orders;
    }

    private sealed record CatalogItem(
        int Id,
        string Name,
        decimal Amount,
        string Currency,
        IReadOnlyList<string> Tags,
        string Customer,
        string Status);
}
