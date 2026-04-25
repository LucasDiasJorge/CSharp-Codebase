namespace UseCases.Examples.ProcessOrder.DTOs;

/// <summary>
/// DTO de entrada para processamento de pedido
/// </summary>
public record ProcessOrderInput(
    Guid CustomerId,
    List<OrderItemInput> Items,
    string ShippingAddress,
    PaymentMethod PaymentMethod
);
