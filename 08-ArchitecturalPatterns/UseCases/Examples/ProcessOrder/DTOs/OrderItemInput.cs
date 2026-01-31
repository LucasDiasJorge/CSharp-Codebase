namespace UseCases.Examples.ProcessOrder.DTOs;

/// <summary>
/// DTO de item do pedido
/// </summary>
public record OrderItemInput(
    Guid ProductId,
    int Quantity
);
