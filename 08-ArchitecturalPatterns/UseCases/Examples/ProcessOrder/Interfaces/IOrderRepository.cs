using UseCases.Examples.ProcessOrder.Entities;

namespace UseCases.Examples.ProcessOrder.Interfaces;

/// <summary>
/// Repositório de pedidos
/// </summary>
public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
}
