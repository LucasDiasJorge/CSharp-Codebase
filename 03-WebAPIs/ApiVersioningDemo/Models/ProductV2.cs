namespace ApiVersioningDemo.Models;

/// <summary>
/// Contrato da v2. Duas mudanças com naturezas diferentes:
/// <c>price</c> virou objeto (quebra o cliente da v1 — foi o que exigiu a nova versão)
/// e <c>tags</c> é campo novo (aditivo, caberia na própria v1).
/// </summary>
public sealed class ProductV2
{
    public ProductV2(int id, string name, MoneyV2 price, IReadOnlyList<string> tags)
    {
        Id = id;
        Name = name;
        Price = price;
        Tags = tags;
    }

    public int Id { get; }

    public string Name { get; }

    public MoneyV2 Price { get; }

    public IReadOnlyList<string> Tags { get; }
}

/// <summary>Valor monetário com moeda explícita.</summary>
public sealed class MoneyV2
{
    public MoneyV2(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }
}
