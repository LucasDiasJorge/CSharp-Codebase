using PersistencePatterns.Core;
using PersistencePatterns.Examples.Repository.Entities;

namespace PersistencePatterns.Examples.Repository.Interfaces;

/// <summary>
/// Interface específica do repositório de produtos
/// Estende a interface genérica com métodos específicos do domínio
/// </summary>
public interface IProductRepository : IRepository<Product, Guid>
{
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold, CancellationToken ct = default);
    Task<Product?> GetByNameAsync(string name, CancellationToken ct = default);
}
