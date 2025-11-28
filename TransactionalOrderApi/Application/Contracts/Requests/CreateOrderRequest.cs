namespace TransactionalOrderApi.Application.Contracts.Requests;

public record CreateOrderRequest
(
    CustomerRequest Customer,
    IReadOnlyCollection<CreateOrderItemRequest> Items,
    string Currency
)
{
    public void EnsureIsValid()
    {
        if (Customer is null)
        {
            throw new ArgumentException("Customer payload is required.");
        }

        if (Items is null || Items.Count == 0)
        {
            throw new ArgumentException("At least one order item is required.");
        }

        if (Items.Any(i => i.Quantity <= 0 || i.UnitPrice <= 0))
        {
            throw new ArgumentException("Quantity and price must be positive numbers.");
        }

        if (string.IsNullOrWhiteSpace(Currency))
        {
            throw new ArgumentException("Currency is required.");
        }
    }
}

public record CustomerRequest(string FullName, string Email);

public record CreateOrderItemRequest(string Sku, int Quantity, decimal UnitPrice);
