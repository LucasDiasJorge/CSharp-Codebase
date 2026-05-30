namespace VerticalSliceMinimalApi.Features.Orders.Create;

public static class CreateOrderEndpoint
{
    public static RouteHandlerBuilder MapCreateOrder(this RouteGroupBuilder group)
    {
        return group.MapPost("/", HandleAsync)
            .WithName("CreateOrder")
            .WithSummary("Cria um novo pedido.")
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static Task<IResult> HandleAsync(
        CreateOrderRequest request,
        CreateOrderHandler handler,
        CancellationToken cancellationToken)
    {
        return handler.HandleAsync(request, cancellationToken);
    }
}
