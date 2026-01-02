namespace SagaPattern.Examples.OrderSaga.Context;

/// <summary>
/// Contexto compartilhado entre todos os passos da Saga de Pedido
/// </summary>
public class OrderSagaContext
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? ShippingTrackingCode { get; set; }
    public bool StockReserved { get; set; }
    public bool PaymentProcessed { get; set; }
    public bool OrderCreated { get; set; }
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
