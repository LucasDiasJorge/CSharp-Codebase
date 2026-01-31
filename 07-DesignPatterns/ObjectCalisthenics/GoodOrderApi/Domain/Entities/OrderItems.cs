using GoodOrderApi.Domain.ValueObjects;

namespace GoodOrderApi.Domain.Entities;

// ✅ REGRA 4: First Class Collections - Coleção encapsulada
// ✅ REGRA 5: Um ponto por linha - Encapsula operações na coleção

/// <summary>
/// First Class Collection que encapsula a lista de itens do pedido.
/// Toda lógica relacionada à coleção está aqui.
/// </summary>
public sealed class OrderItems
{
    private readonly List<OrderItem> _items;

    private OrderItems()
    {
        _items = new List<OrderItem>();
    }

    public static OrderItems Empty() => new();

    public int Count => _items.Count;
    
    public bool HasItems => _items.Count > 0;

    public IReadOnlyList<OrderItem> ToList() => _items.AsReadOnly();

    public OrderItems Add(OrderItem item)
    {
        var newCollection = new OrderItems();
        newCollection._items.AddRange(_items);
        newCollection._items.Add(item);
        return newCollection;
    }

    public Money CalculateTotal()
    {
        return _items.Aggregate(
            Money.Zero, 
            (total, item) => total.Add(item.CalculateTotal())
        );
    }

    public int CalculateTotalQuantity()
    {
        return _items.Sum(item => item.GetQuantity().Value);
    }

    // ✅ Expõe comportamento através de método que executa ação
    public void ForEach(Action<OrderItem> action)
    {
        foreach (var item in _items)
        {
            action(item);
        }
    }
}
