namespace TestDoublesDemo.Domain;

public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice)
{
    public decimal Total => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Draft,
    Confirmed,
    Rejected,
}

/// <summary>
/// O pedido. Deliberadamente simples: o assunto deste exemplo são os dublês, não a
/// modelagem do domínio.
/// </summary>
public sealed class Order
{
    private readonly List<OrderLine> _lines = new List<OrderLine>();

    public Order(Guid id, string customerEmail)
    {
        Id = id;
        CustomerEmail = customerEmail;
    }

    public Guid Id { get; }

    public string CustomerEmail { get; }

    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public IReadOnlyList<OrderLine> Lines => _lines;

    public decimal Total => _lines.Sum(line => line.Total);

    public Order AddLine(string sku, int quantity, decimal unitPrice)
    {
        _lines.Add(new OrderLine(sku, quantity, unitPrice));

        return this;
    }

    public void Confirm() => Status = OrderStatus.Confirmed;

    public void Reject() => Status = OrderStatus.Rejected;
}
