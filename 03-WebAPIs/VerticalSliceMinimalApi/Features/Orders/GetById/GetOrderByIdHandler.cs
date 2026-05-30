using Microsoft.AspNetCore.Mvc;
using VerticalSliceMinimalApi.Domain;
using VerticalSliceMinimalApi.Infrastructure.Orders;

namespace VerticalSliceMinimalApi.Features.Orders.GetById;

public sealed class GetOrderByIdHandler
{
    private readonly IOrderRepository orderRepository;

    public GetOrderByIdHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task<IResult> HandleAsync(Int32 id, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            ProblemDetails problem = new ProblemDetails
            {
                Title = "Pedido nao encontrado",
                Detail = $"Nao existe pedido com id {id}.",
                Status = StatusCodes.Status404NotFound
            };

            return Results.NotFound(problem);
        }

        GetOrderByIdResponse response = new GetOrderByIdResponse(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.CreatedAtUtc);

        return Results.Ok(response);
    }
}
