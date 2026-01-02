using UseCases.Examples.ProcessOrder.Entities;

namespace UseCases.Examples.ProcessOrder.DTOs;

/// <summary>
/// DTO de saída do processamento de pedido
/// </summary>
public record ProcessOrderOutput(
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmount,
    decimal Discount,
    decimal FinalAmount,
    DateTime EstimatedDelivery,
    OrderStatus Status
);
