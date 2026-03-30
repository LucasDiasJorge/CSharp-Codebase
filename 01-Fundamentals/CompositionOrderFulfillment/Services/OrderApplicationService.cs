using CompositionOrderFulfillment.Models;

namespace CompositionOrderFulfillment.Services;

public sealed class OrderApplicationService
{
    private readonly List<Product> catalog;

    public OrderApplicationService()
    {
        catalog = BuildDefaultCatalog();
    }

    public IReadOnlyCollection<Product> GetCatalog()
    {
        return catalog.AsReadOnly();
    }

    public Product GetProductBySku(string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        foreach (Product product in catalog)
        {
            bool sameSku = product.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase);

            if (sameSku)
            {
                return product;
            }
        }

        throw new InvalidOperationException($"Produto {sku} nao encontrado no catalogo.");
    }

    public PurchaseOrder StartOrder(string customerName)
    {
        return new PurchaseOrder(customerName);
    }

    private static List<Product> BuildDefaultCatalog()
    {
        List<Product> defaultCatalog = new List<Product>
        {
            new Product("KB-100", "Teclado Mecanico", 350.00m),
            new Product("MS-200", "Mouse Gamer", 210.50m),
            new Product("HS-300", "Headset USB", 420.90m)
        };

        return defaultCatalog;
    }
}