namespace VerticalSliceMinimalApi.Features.Orders.GetById;

public sealed record GetOrderByIdResponse(
    Int32 Id,
    String CustomerName,
    Decimal TotalAmount,
    DateTime CreatedAtUtc);
