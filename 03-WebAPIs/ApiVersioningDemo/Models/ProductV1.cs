namespace ApiVersioningDemo.Models;

/// <summary>
/// Contrato da v1: preço é um número solto, sem moeda. A limitação que motivou a v2.
/// </summary>
public sealed class ProductV1
{
    public ProductV1(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    public int Id { get; }

    public string Name { get; }

    public decimal Price { get; }
}
