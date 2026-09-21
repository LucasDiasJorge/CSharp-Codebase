using System.Collections.Concurrent;

namespace Modules.Orders.Internal;

/// <summary>Armazenamento próprio do módulo de Pedidos. <c>internal</c>: ninguém de fora alcança.</summary>
internal sealed class OrdersDatabase
{
    private readonly ConcurrentDictionary<string, OrderEntity> _orders = new ConcurrentDictionary<string, OrderEntity>();

    public void Save(OrderEntity order) => _orders[order.OrderId] = order;

    public OrderEntity? Find(string orderId) => _orders.TryGetValue(orderId, out OrderEntity? order) ? order : null;

    public IReadOnlyCollection<OrderEntity> All() => _orders.Values.ToArray();
}

internal sealed class OrderEntity
{
    public OrderEntity(string orderId, string customerId, string sku, int quantity, decimal total)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Sku = sku;
        Quantity = quantity;
        Total = total;
        Status = "aguardando";
    }

    public string OrderId { get; }

    public string CustomerId { get; }

    public string Sku { get; }

    public int Quantity { get; }

    public decimal Total { get; }

    public string Status { get; private set; }

    public bool StockReserved { get; private set; }

    public bool PaymentConfirmed { get; private set; }

    public void MarkStockReserved()
    {
        StockReserved = true;
        Advance();
    }

    public void MarkPaid()
    {
        PaymentConfirmed = true;
        Advance();
    }

    /// <summary>
    /// Cancela o pedido. A PRIMEIRA causa vence: dois módulos podem rejeitar o mesmo
    /// pedido de forma independente (sem estoque e valor acima do limite), e sobrescrever
    /// o motivo apagaria a causa real, que foi a primeira a chegar.
    /// </summary>
    public void Fail(string reason)
    {
        if (Status.StartsWith("cancelado", StringComparison.Ordinal))
        {
            return;
        }

        Status = $"cancelado ({reason})";
    }

    /// <summary>
    /// O pedido só é confirmado quando as duas confirmações chegam. Como os eventos são
    /// independentes, a ordem em que chegam não pode importar.
    /// </summary>
    private void Advance()
    {
        if (StockReserved && PaymentConfirmed)
        {
            Status = "confirmado";
        }
    }
}
