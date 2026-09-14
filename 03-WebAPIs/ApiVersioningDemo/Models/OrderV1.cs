namespace ApiVersioningDemo.Models;

/// <summary>
/// Contrato da v1 de pedidos. O campo <c>client</c> foi renomeado na v2 — renomear é
/// tão quebra de contrato quanto remover.
/// </summary>
public sealed class OrderV1
{
    public OrderV1(int id, string client, decimal total)
    {
        Id = id;
        Client = client;
        Total = total;
    }

    public int Id { get; }

    public string Client { get; }

    public decimal Total { get; }
}
