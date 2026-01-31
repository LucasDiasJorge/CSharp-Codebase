using CarriedEvent.Core;
using CarriedEvent.Examples.OrderCreated.Events;

namespace CarriedEvent.Examples.OrderCreated.Handlers;

/// <summary>
/// Handler de Inventário - Atualiza estoque
/// Usa os dados dos produtos carregados no evento
/// </summary>
public class InventoryHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine("\n  [InventoryHandler] Processando evento...");

        // Não precisa buscar dados dos produtos - já estão no evento!
        foreach (var item in @event.Items)
        {
            Console.WriteLine($"    📦 Reservando estoque:");
            Console.WriteLine($"       Produto: {item.ProductName}");
            Console.WriteLine($"       SKU: {item.ProductSku}");
            Console.WriteLine($"       Quantidade: {item.Quantity}");
        }

        return Task.CompletedTask;
    }
}
