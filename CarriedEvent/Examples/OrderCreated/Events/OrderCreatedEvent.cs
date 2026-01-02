using CarriedEvent.Core;

namespace CarriedEvent.Examples.OrderCreated.Events;

/// <summary>
/// Evento que carrega todo o estado do pedido criado
/// Consumidores não precisam consultar o serviço de pedidos
/// </summary>
public class OrderCreatedEvent : CarriedStateEvent
{
    public override string EventType => "order.created";

    // Dados do Pedido
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public string Status { get; init; } = string.Empty;

    // Dados do Cliente (carried state)
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;

    // Endereço de Entrega (carried state)
    public ShippingAddressData ShippingAddress { get; init; } = new();

    // Itens do Pedido (carried state)
    public List<OrderItemData> Items { get; init; } = [];

    // Valores
    public decimal SubTotal { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal Discount { get; init; }
    public decimal TotalAmount { get; init; }

    // Pagamento
    public string PaymentMethod { get; init; } = string.Empty;
}

public class ShippingAddressData
{
    public string Street { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public class OrderItemData
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSku { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}
