using Microsoft.EntityFrameworkCore;
using TransactionalOrderApi.Domain.Entities;
using TransactionalOrderApi.Infrastructure.Persistence;

namespace TransactionalOrderApi.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext context) : ICustomerRepository
{
    private readonly AppDbContext _context = context;

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        _context.Customers.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        return customer;
    }
}
