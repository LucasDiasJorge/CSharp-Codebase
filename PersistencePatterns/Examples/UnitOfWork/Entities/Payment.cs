using PersistencePatterns.Core;

namespace PersistencePatterns.Examples.UnitOfWork.Entities;

/// <summary>
/// Entidade Payment
/// </summary>
public class Payment : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, decimal amount)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            ProcessedAt = DateTime.UtcNow
        };
    }

    public void Approve() => Status = PaymentStatus.Approved;
    public void Reject() => Status = PaymentStatus.Rejected;
}
