namespace TransactionalOutboxDemo.Domain;

/// <summary>
/// Entidade de negócio. O que importa no exemplo é que ela e a mensagem de saída são
/// gravadas na mesma transação — nunca uma sem a outra.
/// </summary>
public sealed class Order
{
    public Order(string customer, decimal total)
    {
        Customer = customer;
        Total = total;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    private Order()
    {
        Customer = string.Empty;
    }

    public int Id { get; private set; }

    public string Customer { get; private set; }

    public decimal Total { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
