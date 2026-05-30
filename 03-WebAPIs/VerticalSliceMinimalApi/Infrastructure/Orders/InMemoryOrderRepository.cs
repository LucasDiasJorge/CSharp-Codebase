using System.Collections.Concurrent;
using VerticalSliceMinimalApi.Domain;

namespace VerticalSliceMinimalApi.Infrastructure.Orders;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Int32, Order> orders;
    private Int32 currentId;

    public InMemoryOrderRepository()
    {
        orders = new ConcurrentDictionary<Int32, Order>();
    }

    public Task<Order> AddAsync(String customerName, Decimal totalAmount, CancellationToken cancellationToken)
    {
        Int32 nextId = Interlocked.Increment(ref currentId);
        Order order = new Order(nextId, customerName, totalAmount, DateTime.UtcNow);
        Boolean added = orders.TryAdd(nextId, order);

        if (!added)
        {
            throw new InvalidOperationException("Falha ao persistir o pedido em memoria.");
        }

        return Task.FromResult(order);
    }

    public Task<Order?> GetByIdAsync(Int32 id, CancellationToken cancellationToken)
    {
        Order? order;
        Boolean found = orders.TryGetValue(id, out order);

        if (!found)
        {
            return Task.FromResult<Order?>(null);
        }

        return Task.FromResult(order);
    }

    public Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken)
    {
        List<Order> sortedOrders = orders.Values
            .OrderBy(order => order.CreatedAtUtc)
            .ToList();

        IReadOnlyCollection<Order> result = sortedOrders;
        return Task.FromResult(result);
    }
}
