using UseCases.Examples.ProcessOrder.Entities;

namespace UseCases.Examples.ProcessOrder.Interfaces;

/// <summary>
/// Repositório de produtos
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}
