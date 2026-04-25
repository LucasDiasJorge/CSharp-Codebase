using System.Linq.Expressions;
using PersistencePatterns.Examples.Repository.Entities;
using PersistencePatterns.Examples.Repository.Interfaces;

namespace PersistencePatterns.Examples.Repository.Implementations;

/// <summary>
/// Implementação em memória do repositório de produtos
/// Em produção, seria substituído por EF Core, Dapper, etc.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = [];

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IEnumerable<Product>>(_products.ToList());
    }

    public Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate, CancellationToken ct = default)
    {
        var compiled = predicate.Compile();
        var results = _products.Where(compiled);
        return Task.FromResult(results);
    }

    public Task AddAsync(Product entity, CancellationToken ct = default)
    {
        _products.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product entity, CancellationToken ct = default)
    {
        var index = _products.FindIndex(p => p.Id == entity.Id);
        if (index >= 0)
            _products[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product entity, CancellationToken ct = default)
    {
        _products.RemoveAll(p => p.Id == entity.Id);
        return Task.CompletedTask;
    }

    // Métodos específicos do domínio
    public Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken ct = default)
    {
        var actives = _products.Where(p => p.IsActive);
        return Task.FromResult(actives);
    }

    public Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken ct = default)
    {
        var results = _products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold, CancellationToken ct = default)
    {
        var lowStock = _products.Where(p => p.StockQuantity <= threshold && p.IsActive);
        return Task.FromResult(lowStock);
    }

    public Task<Product?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var product = _products.FirstOrDefault(p => 
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(product);
    }
}
