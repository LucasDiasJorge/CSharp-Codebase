using TransactionalOrderApi.Domain.Entities;

namespace TransactionalOrderApi.Application.Contracts.Responses;

public record OrderResponse
(
    int Id,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAtUtc,
    CustomerResponse Customer,
    IReadOnlyCollection<OrderItemResponse> Items
)
{
    public static OrderResponse From(Order order) => new(
        order.Id,
        order.Status.ToString(),
        order.TotalAmount,
        order.Currency,
        order.CreatedAtUtc,
        new CustomerResponse(order.Customer.Id, order.Customer.FullName, order.Customer.Email, order.Customer.TotalSpent),
        order.Items.Select(i => new OrderItemResponse(i.Id, i.Sku, i.Quantity, i.UnitPrice, i.Subtotal)).ToList()
    );
}

public record CustomerResponse(int Id, string FullName, string Email, decimal TotalSpent);

public record OrderItemResponse(int Id, string Sku, int Quantity, decimal UnitPrice, decimal Subtotal);
