using PersistencePatterns.Core;

namespace PersistencePatterns.Examples.UnitOfWork.Entities;

/// <summary>
/// Entidade Order para demonstração do Unit of Work
/// </summary>
public class Order : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    private Order() { }

    public static Order Create(Guid customerId, decimal totalAmount)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount
        };
    }

    public void Confirm() => Status = OrderStatus.Confirmed;
    public void Ship() => Status = OrderStatus.Shipped;
    public void Cancel() => Status = OrderStatus.Cancelled;
}
