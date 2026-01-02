using System.Linq.Expressions;
using PersistencePatterns.Examples.UnitOfWork.Entities;
using PersistencePatterns.Examples.UnitOfWork.Interfaces;

namespace PersistencePatterns.Examples.UnitOfWork.Implementations;

/// <summary>
/// Implementação em memória do repositório de pagamentos
/// </summary>
public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly List<Payment> _payments;

    public InMemoryPaymentRepository(List<Payment> payments)
    {
        _payments = payments;
    }

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_payments.FirstOrDefault(p => p.Id == id));

    public Task<IEnumerable<Payment>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IEnumerable<Payment>>(_payments.ToList());

    public Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate, CancellationToken ct = default)
        => Task.FromResult(_payments.Where(predicate.Compile()));

    public Task AddAsync(Payment entity, CancellationToken ct = default)
    {
        _payments.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Payment entity, CancellationToken ct = default)
    {
        var index = _payments.FindIndex(p => p.Id == entity.Id);
        if (index >= 0) _payments[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Payment entity, CancellationToken ct = default)
    {
        _payments.RemoveAll(p => p.Id == entity.Id);
        return Task.CompletedTask;
    }

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => Task.FromResult(_payments.FirstOrDefault(p => p.OrderId == orderId));
}
