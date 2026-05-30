namespace VerticalSliceMinimalApi.Features.Orders.List;

public static class ListOrdersEndpoint
{
    public static RouteHandlerBuilder MapListOrders(this RouteGroupBuilder group)
    {
        return group.MapGet("/", HandleAsync)
            .WithName("ListOrders")
            .WithSummary("Lista todos os pedidos.")
            .Produces<ListOrdersResponse>(StatusCodes.Status200OK);
    }

    private static Task<IResult> HandleAsync(ListOrdersHandler handler, CancellationToken cancellationToken)
    {
        return handler.HandleAsync(cancellationToken);
    }
}
