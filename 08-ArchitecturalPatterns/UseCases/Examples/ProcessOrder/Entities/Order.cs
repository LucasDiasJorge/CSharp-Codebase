using UseCases.Examples.ProcessOrder.DTOs;

namespace UseCases.Examples.ProcessOrder.Entities;

/// <summary>
/// Entidade de domínio: Pedido
/// </summary>
public class Order
{
    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];
    public decimal TotalAmount { get; private set; }
    public decimal Discount { get; private set; }
    public decimal FinalAmount { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime EstimatedDelivery { get; private set; }

    private Order() { }

    public static Order Create(
        Guid customerId,
        string shippingAddress,
        PaymentMethod paymentMethod)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            PaymentMethod = paymentMethod,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EstimatedDelivery = CalculateEstimatedDelivery(paymentMethod)
        };
    }

    public void AddItem(Product product, int quantity)
    {
        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = quantity,
            TotalPrice = product.Price * quantity
        };
        Items.Add(item);
        RecalculateTotals();
    }

    public void ApplyDiscount(decimal discountPercentage)
    {
        Discount = TotalAmount * (discountPercentage / 100);
        FinalAmount = TotalAmount - Discount;
    }

    public void ConfirmPayment()
    {
        Status = OrderStatus.PaymentConfirmed;
    }

    private void RecalculateTotals()
    {
        TotalAmount = Items.Sum(i => i.TotalPrice);
        FinalAmount = TotalAmount - Discount;
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static DateTime CalculateEstimatedDelivery(PaymentMethod method)
    {
        var days = method switch
        {
            PaymentMethod.Pix => 3,
            PaymentMethod.CreditCard => 5,
            PaymentMethod.DebitCard => 5,
            PaymentMethod.BankSlip => 10,
            _ => 7
        };
        return DateTime.UtcNow.AddDays(days);
    }
}
