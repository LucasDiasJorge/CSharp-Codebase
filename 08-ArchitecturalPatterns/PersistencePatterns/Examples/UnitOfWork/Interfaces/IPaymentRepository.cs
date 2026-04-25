using PersistencePatterns.Core;
using PersistencePatterns.Examples.UnitOfWork.Entities;

namespace PersistencePatterns.Examples.UnitOfWork.Interfaces;

/// <summary>
/// Repositório de pagamentos
/// </summary>
public interface IPaymentRepository : IRepository<Payment, Guid>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
