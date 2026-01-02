using CarriedEvent.Core;
using CarriedEvent.Examples.OrderCreated.Events;

namespace CarriedEvent.Examples.OrderCreated.Handlers;

/// <summary>
/// Handler de Logística - Cria ordem de envio
/// Usa os dados de endereço carregados no evento
/// </summary>
public class ShippingHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine("\n  [ShippingHandler] Processando evento...");

        // Não precisa buscar endereço do cliente - já está no evento!
        var address = @event.ShippingAddress;
        Console.WriteLine($"    🚚 Criando ordem de envio:");
        Console.WriteLine($"       Pedido: {@event.OrderNumber}");
        Console.WriteLine($"       Destinatário: {@event.CustomerName}");
        Console.WriteLine($"       Endereço: {address.Street}, {address.Number}");
        Console.WriteLine($"       Cidade: {address.City} - {address.State}");
        Console.WriteLine($"       CEP: {address.ZipCode}");
        Console.WriteLine($"       Itens: {@event.Items.Count}");

        return Task.CompletedTask;
    }
}
