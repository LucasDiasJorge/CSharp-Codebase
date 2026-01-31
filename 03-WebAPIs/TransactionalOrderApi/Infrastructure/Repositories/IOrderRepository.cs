using TransactionalOrderApi.Domain.Entities;

namespace TransactionalOrderApi.Infrastructure.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken);
}
