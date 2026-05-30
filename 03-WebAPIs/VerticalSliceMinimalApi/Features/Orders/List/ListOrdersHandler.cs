using VerticalSliceMinimalApi.Domain;
using VerticalSliceMinimalApi.Infrastructure.Orders;

namespace VerticalSliceMinimalApi.Features.Orders.List;

public sealed class ListOrdersHandler
{
    private readonly IOrderRepository orderRepository;

    public ListOrdersHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task<IResult> HandleAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Order> orders = await orderRepository.ListAsync(cancellationToken);

        IReadOnlyCollection<ListOrdersItemResponse> items = orders
            .Select(order => new ListOrdersItemResponse(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.CreatedAtUtc))
            .ToList();

        ListOrdersResponse response = new ListOrdersResponse(items.Count, items);

        return Results.Ok(response);
    }
}
