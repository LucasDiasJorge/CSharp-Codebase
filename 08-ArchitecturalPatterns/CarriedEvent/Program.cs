using CarriedEvent.Core;
using CarriedEvent.Examples.OrderCreated.Events;
using CarriedEvent.Examples.OrderCreated.Handlers;

namespace CarriedEvent;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       EVENT CARRIED STATE TRANSFER EM C#                  ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");

        await DemonstrateCarriedEvent();

        Console.WriteLine("\n✅ Demonstração concluída!");
    }

    static async Task DemonstrateCarriedEvent()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("📦 EVENTO COM ESTADO CARREGADO");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        // Configurar Event Bus
        var eventBus = new InMemoryEventBus();

        // Registrar múltiplos handlers
        Console.WriteLine("\nRegistrando handlers:");
        eventBus.Subscribe(new NotificationHandler());
        eventBus.Subscribe(new InventoryHandler());
        eventBus.Subscribe(new ShippingHandler());
        eventBus.Subscribe(new AnalyticsHandler());

        // Criar evento com todos os dados necessários
        var orderEvent = new OrderCreatedEvent
        {
            // Dados do Pedido
            OrderId = Guid.NewGuid(),
            OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-001",
            OrderDate = DateTime.UtcNow,
            Status = "Confirmed",

            // Dados do Cliente
            CustomerId = Guid.NewGuid(),
            CustomerName = "João Silva",
            CustomerEmail = "joao.silva@email.com",
            CustomerPhone = "(11) 99999-9999",

            // Endereço de Entrega
            ShippingAddress = new ShippingAddressData
            {
                Street = "Rua das Flores",
                Number = "123",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01234-567",
                Country = "Brasil"
            },

            // Itens
            Items =
            [
                new OrderItemData
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Notebook Dell Inspiron",
                    ProductSku = "DELL-INS-15-001",
                    Quantity = 1,
                    UnitPrice = 3500.00m,
                    TotalPrice = 3500.00m
                },
                new OrderItemData
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Mouse Logitech MX Master",
                    ProductSku = "LOG-MX-M3-001",
                    Quantity = 2,
                    UnitPrice = 450.00m,
                    TotalPrice = 900.00m
                }
            ],

            // Valores
            SubTotal = 4400.00m,
            ShippingCost = 50.00m,
            Discount = 200.00m,
            TotalAmount = 4250.00m,

            // Pagamento
            PaymentMethod = "Cartão de Crédito"
        };

        // Publicar evento
        Console.WriteLine("\n───────────────────────────────────────────────────────────");
        Console.WriteLine("Publicando evento OrderCreated...");
        Console.WriteLine("───────────────────────────────────────────────────────────");

        await eventBus.PublishAsync(orderEvent);

        Console.WriteLine("\n───────────────────────────────────────────────────────────");
        Console.WriteLine("✅ Todos os handlers processaram o evento com sucesso!");
        Console.WriteLine("   Nenhuma chamada adicional foi necessária.");
        Console.WriteLine("───────────────────────────────────────────────────────────");
    }
}
