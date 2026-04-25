using CarriedEvent.Core;
using CarriedEvent.Examples.OrderCreated.Events;

namespace CarriedEvent.Examples.OrderCreated.Handlers;

/// <summary>
/// Handler de Notificação - Envia email para o cliente
/// Usa os dados do cliente carregados no evento
/// </summary>
public class NotificationHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine("\n  [NotificationHandler] Processando evento...");
        
        // Não precisa buscar dados do cliente - já estão no evento!
        Console.WriteLine($"    📧 Enviando email para: {@event.CustomerEmail}");
        Console.WriteLine($"    📧 Destinatário: {@event.CustomerName}");
        Console.WriteLine($"    📧 Assunto: Pedido {@event.OrderNumber} confirmado!");
        Console.WriteLine($"    📧 Total: {@event.TotalAmount:C}");

        return Task.CompletedTask;
    }
}
