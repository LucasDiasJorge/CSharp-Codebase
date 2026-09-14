namespace ApiVersioningDemo.Models;

/// <summary>
/// Contrato da v2 de pedidos: <c>client</c> virou <c>customer</c> e <c>status</c> foi
/// acrescentado. Mesma rota da v1 — quem escolhe é o header ou a query string.
/// </summary>
public sealed class OrderV2
{
    public OrderV2(int id, string customer, decimal total, string status)
    {
        Id = id;
        Customer = customer;
        Total = total;
        Status = status;
    }

    public int Id { get; }

    public string Customer { get; }

    public decimal Total { get; }

    public string Status { get; }
}
