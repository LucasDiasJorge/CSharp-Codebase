using TransactionalOrderApi.Application.Contracts.Requests;
using TransactionalOrderApi.Application.Contracts.Responses;
using TransactionalOrderApi.Domain.Entities;
using TransactionalOrderApi.Infrastructure.Repositories;

namespace TransactionalOrderApi.Application.Services;

public class OrderService(
    ITransactionRunner transactionRunner,
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository) : IOrderService
{
    private readonly ITransactionRunner _transactionRunner = transactionRunner;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task<OrderResponse> PlaceOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        request.EnsureIsValid();

        return await _transactionRunner.ExecuteAsync(async (_, token) =>
        {
            Customer customer = await _customerRepository.GetByEmailAsync(request.Customer.Email, token)
                ?? await _customerRepository.AddAsync(new Customer(request.Customer.FullName, request.Customer.Email), token);

            var order = Order.Create(customer, request.Currency);
            foreach (var item in request.Items)
            {
                order.AddItem(item.Sku, item.Quantity, item.UnitPrice);
            }

            order.MarkAsCompleted();
            customer.RegisterPurchase(order.TotalAmount);

            await _orderRepository.AddAsync(order, token);

            return OrderResponse.From(order);
        }, cancellationToken);
    }

    public async Task<OrderResponse?> GetOrderAsync(int id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : OrderResponse.From(order);
    }
}
