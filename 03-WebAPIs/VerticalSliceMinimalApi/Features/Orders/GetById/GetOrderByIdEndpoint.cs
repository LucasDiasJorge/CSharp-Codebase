namespace VerticalSliceMinimalApi.Features.Orders.GetById;

public static class GetOrderByIdEndpoint
{
    public static RouteHandlerBuilder MapGetOrderById(this RouteGroupBuilder group)
    {
        return group.MapGet("/{id:int}", HandleAsync)
            .WithName("GetOrderById")
            .WithSummary("Busca um pedido por id.")
            .Produces<GetOrderByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static Task<IResult> HandleAsync(Int32 id, GetOrderByIdHandler handler, CancellationToken cancellationToken)
    {
        return handler.HandleAsync(id, cancellationToken);
    }
}
