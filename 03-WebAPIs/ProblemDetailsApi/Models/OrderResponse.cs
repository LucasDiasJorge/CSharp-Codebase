namespace ProblemDetailsApi.Models;

public sealed class OrderResponse
{
    public OrderResponse(int id, string sku, int quantity, string status)
    {
        Id = id;
        Sku = sku;
        Quantity = quantity;
        Status = status;
    }

    public int Id { get; }

    public string Sku { get; }

    public int Quantity { get; }

    public string Status { get; }
}
