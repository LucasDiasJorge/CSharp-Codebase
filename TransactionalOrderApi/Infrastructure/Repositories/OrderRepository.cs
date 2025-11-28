using Microsoft.EntityFrameworkCore;
using TransactionalOrderApi.Domain.Entities;
using TransactionalOrderApi.Infrastructure.Persistence;

namespace TransactionalOrderApi.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    private readonly AppDbContext _context = context;

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        return order;
    }
}
