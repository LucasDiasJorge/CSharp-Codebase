using System.Linq.Expressions;
using PersistencePatterns.Examples.UnitOfWork.Entities;
using PersistencePatterns.Examples.UnitOfWork.Interfaces;

namespace PersistencePatterns.Examples.UnitOfWork.Implementations;

/// <summary>
/// Implementação em memória do repositório de pedidos
/// </summary>
public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders;

    public InMemoryOrderRepository(List<Order> orders)
    {
        _orders = orders;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));

    public Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IEnumerable<Order>>(_orders.ToList());

    public Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate, CancellationToken ct = default)
        => Task.FromResult(_orders.Where(predicate.Compile()));

    public Task AddAsync(Order entity, CancellationToken ct = default)
    {
        _orders.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order entity, CancellationToken ct = default)
    {
        var index = _orders.FindIndex(o => o.Id == entity.Id);
        if (index >= 0) _orders[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Order entity, CancellationToken ct = default)
    {
        _orders.RemoveAll(o => o.Id == entity.Id);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
        => Task.FromResult(_orders.Where(o => o.CustomerId == customerId));

    public Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken ct = default)
        => Task.FromResult(_orders.Where(o => o.Status == OrderStatus.Pending));
}
