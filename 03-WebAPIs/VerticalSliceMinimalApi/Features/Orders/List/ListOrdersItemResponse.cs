namespace VerticalSliceMinimalApi.Features.Orders.List;

public sealed record ListOrdersItemResponse(
    Int32 Id,
    String CustomerName,
    Decimal TotalAmount,
    DateTime CreatedAtUtc);
