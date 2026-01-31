using CQRSDemo.Models;

namespace CQRSDemo.Infrastructure;

/// <summary>
/// Simula um banco de dados em memória
/// Em um projeto real, você usaria Entity Framework Core ou outro ORM
/// </summary>
public class InMemoryDatabase
{
    private readonly List<Product> _products = new();
    private static readonly object _lock = new();

    public List<Product> GetAllProducts()
    {
        lock (_lock)
        {
            return _products.ToList();
        }
    }

    public Product? GetProductById(Guid id)
    {
        lock (_lock)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }
    }

    public void AddProduct(Product product)
    {
        lock (_lock)
        {
            _products.Add(product);
        }
    }

    public bool UpdateProduct(Product product)
    {
        lock (_lock)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct == null)
                return false;

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.UpdatedAt = DateTime.UtcNow;
            
            return true;
        }
    }

    public bool DeleteProduct(Guid id)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return false;

            _products.Remove(product);
            return true;
        }
    }
}
