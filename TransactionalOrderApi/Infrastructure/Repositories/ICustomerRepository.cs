using TransactionalOrderApi.Domain.Entities;

namespace TransactionalOrderApi.Infrastructure.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken);
}
