using PersistencePatterns.Examples.IdentityMap.Entities;

namespace PersistencePatterns.Examples.IdentityMap.Interfaces;

/// <summary>
/// Repositório de clientes com suporte a Identity Map
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
    void ClearCache();
}
