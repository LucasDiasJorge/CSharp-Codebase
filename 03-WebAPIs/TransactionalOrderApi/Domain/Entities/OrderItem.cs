namespace TransactionalOrderApi.Domain.Entities;

public class OrderItem
{
    private OrderItem() { }

    internal OrderItem(Order order, string sku, int quantity, decimal unitPrice)
    {
        Order = order;
        OrderId = order.Id;
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Subtotal = quantity * unitPrice;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public string Sku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal { get; private set; }
}
