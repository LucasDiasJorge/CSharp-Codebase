using TransactionalOrderApi.Application.Contracts.Requests;
using TransactionalOrderApi.Application.Contracts.Responses;

namespace TransactionalOrderApi.Application.Services;

public interface IOrderService
{
    Task<OrderResponse> PlaceOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderResponse?> GetOrderAsync(int id, CancellationToken cancellationToken);
}
