using UseCases.Examples.ProcessOrder.Entities;

namespace UseCases.Examples.ProcessOrder.Interfaces;

/// <summary>
/// Repositório de clientes
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
