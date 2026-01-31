using CarriedEvent.Core;
using CarriedEvent.Examples.OrderCreated.Events;

namespace CarriedEvent.Examples.OrderCreated.Handlers;

/// <summary>
/// Handler de Analytics - Registra métricas
/// Usa todos os dados disponíveis para análise
/// </summary>
public class AnalyticsHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine("\n  [AnalyticsHandler] Processando evento...");

        // Análise completa com todos os dados disponíveis
        Console.WriteLine($"    📊 Registrando métricas:");
        Console.WriteLine($"       Cliente: {@event.CustomerId}");
        Console.WriteLine($"       Região: {@event.ShippingAddress.State}");
        Console.WriteLine($"       Valor: {@event.TotalAmount:C}");
        Console.WriteLine($"       Desconto: {@event.Discount:C}");
        Console.WriteLine($"       Pagamento: {@event.PaymentMethod}");
        Console.WriteLine($"       Produtos vendidos:");
        
        foreach (var item in @event.Items)
        {
            Console.WriteLine($"         - {item.ProductName} x{item.Quantity}");
        }

        return Task.CompletedTask;
    }
}
