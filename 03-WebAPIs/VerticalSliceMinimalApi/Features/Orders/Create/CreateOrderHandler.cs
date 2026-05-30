using VerticalSliceMinimalApi.Domain;
using VerticalSliceMinimalApi.Infrastructure.Orders;

namespace VerticalSliceMinimalApi.Features.Orders.Create;

public sealed class CreateOrderHandler
{
    private readonly IOrderRepository orderRepository;

    public CreateOrderHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task<IResult> HandleAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        String normalizedName = request.CustomerName?.Trim() ?? String.Empty;

        if (String.IsNullOrWhiteSpace(normalizedName))
        {
            Dictionary<String, String[]> errors = new Dictionary<String, String[]>
            {
                ["customerName"] = ["O nome do cliente e obrigatorio."]
            };

            return Results.ValidationProblem(errors);
        }

        if (request.TotalAmount <= 0)
        {
            Dictionary<String, String[]> errors = new Dictionary<String, String[]>
            {
                ["totalAmount"] = ["O total do pedido deve ser maior que zero."]
            };

            return Results.ValidationProblem(errors);
        }

        Order createdOrder = await orderRepository.AddAsync(normalizedName, request.TotalAmount, cancellationToken);

        CreateOrderResponse response = new CreateOrderResponse(
            createdOrder.Id,
            createdOrder.CustomerName,
            createdOrder.TotalAmount,
            createdOrder.CreatedAtUtc);

        return Results.Created($"/orders/{response.Id}", response);
    }
}
