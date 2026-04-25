using GoodOrderApi.Domain.Entities;
using GoodOrderApi.Domain.ValueObjects;

namespace GoodOrderApi.Application.Services;

// ✅ REGRA 7: Classes pequenas e focadas
// ✅ Repositórios encapsulam acesso a dados

/// <summary>
/// Repositório de pedidos.
/// </summary>
public class OrderRepository
{
    private readonly List<Order> _orders = new();
    private int _nextId = 1;

    public Order CreateOrder(CustomerInfo customerInfo)
    {
        var order = Order.Create(_nextId++, customerInfo);
        _orders.Add(order);
        return order;
    }

    public Order? FindById(int id)
    {
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public IReadOnlyList<Order> GetAll()
    {
        return _orders.AsReadOnly();
    }

    public IReadOnlyList<Order> FindByStatus(string status)
    {
        return _orders
            .Where(o => o.State.Status.Name == status)
            .ToList()
            .AsReadOnly();
    }
}

/// <summary>
/// Repositório de produtos.
/// </summary>
public class ProductRepository
{
    private readonly List<Product> _products;

    public ProductRepository()
    {
        _products = InitializeProducts();
    }

    private List<Product> InitializeProducts()
    {
        return new List<Product>
        {
            Product.Create(1, "Laptop", "High performance laptop", 1299.99m, 50, "Electronics"),
            Product.Create(2, "Mouse", "Wireless mouse", 29.99m, 200, "Electronics"),
            Product.Create(3, "Keyboard", "Mechanical keyboard", 79.99m, 100, "Electronics"),
            Product.Create(4, "Monitor", "27 inch 4K monitor", 499.99m, 30, "Electronics"),
            Product.Create(5, "Headphones", "Noise cancelling headphones", 199.99m, 75, "Electronics"),
        };
    }

    public Product? FindById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public Product? FindByName(string name)
    {
        return _products.FirstOrDefault(p => p.Info.Identification.Name == name);
    }

    public IReadOnlyList<Product> GetAll()
    {
        return _products.AsReadOnly();
    }
}
