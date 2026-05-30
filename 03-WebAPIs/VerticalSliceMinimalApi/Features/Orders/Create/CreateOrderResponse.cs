namespace VerticalSliceMinimalApi.Features.Orders.Create;

public sealed record CreateOrderResponse(
    Int32 Id,
    String CustomerName,
    Decimal TotalAmount,
    DateTime CreatedAtUtc);
