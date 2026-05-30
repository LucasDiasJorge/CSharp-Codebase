namespace VerticalSliceMinimalApi.Features.Orders.List;

public sealed record ListOrdersResponse(Int32 TotalItems, IReadOnlyCollection<ListOrdersItemResponse> Items);
