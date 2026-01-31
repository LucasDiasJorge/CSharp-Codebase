namespace TransactionalOrderApi.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    private Order() { }

    private Order(Customer customer, string currency)
    {
        Customer = customer;
        CustomerId = customer.Id;
        Currency = currency;
        CreatedAtUtc = DateTime.UtcNow;
        Status = OrderStatus.Pending;
    }

    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Order Create(Customer customer, string currency)
        => new(customer, currency);

    public void AddItem(string sku, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        var item = new OrderItem(this, sku, quantity, unitPrice);
        _items.Add(item);
        TotalAmount += item.Subtotal;
    }

    public void MarkAsCompleted() => Status = OrderStatus.Completed;
}

public enum OrderStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3
}
