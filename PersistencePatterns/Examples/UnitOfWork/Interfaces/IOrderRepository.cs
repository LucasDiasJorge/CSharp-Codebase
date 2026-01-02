using PersistencePatterns.Core;
using PersistencePatterns.Examples.UnitOfWork.Entities;

namespace PersistencePatterns.Examples.UnitOfWork.Interfaces;

/// <summary>
/// Repositório de pedidos
/// </summary>
public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken ct = default);
}
