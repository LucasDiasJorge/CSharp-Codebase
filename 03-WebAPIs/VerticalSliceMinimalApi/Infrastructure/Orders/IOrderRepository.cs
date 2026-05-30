using VerticalSliceMinimalApi.Domain;

namespace VerticalSliceMinimalApi.Infrastructure.Orders;

public interface IOrderRepository
{
    Task<Order> AddAsync(String customerName, Decimal totalAmount, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(Int32 id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken);
}
