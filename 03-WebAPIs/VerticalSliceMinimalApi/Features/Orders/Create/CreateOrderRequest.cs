namespace VerticalSliceMinimalApi.Features.Orders.Create;

public sealed record CreateOrderRequest(String CustomerName, Decimal TotalAmount);
