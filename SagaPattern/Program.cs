using SagaPattern.Examples.OrderSaga;
using SagaPattern.Examples.OrderSaga.Context;

namespace SagaPattern;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              SAGA PATTERN EM C#                           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");

        await DemonstrateSagaSuccess();
        await DemonstrateSagaWithPaymentFailure();
        await DemonstrateSagaWithShipmentFailure();

        Console.WriteLine("\n✅ Demonstrações concluídas!");
    }

    static async Task DemonstrateSagaSuccess()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("✅ CENÁRIO 1: Saga com Sucesso");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var orchestrator = new OrderSagaOrchestrator();
        var context = CreateSampleContext();

        var result = await orchestrator.ExecuteAsync(context);

        PrintResult(result, context);
    }

    static async Task DemonstrateSagaWithPaymentFailure()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("❌ CENÁRIO 2: Saga com Falha no Pagamento");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var orchestrator = new OrderSagaOrchestrator(paymentShouldFail: true);
        var context = CreateSampleContext();

        var result = await orchestrator.ExecuteAsync(context);

        PrintResult(result, context);
    }

    static async Task DemonstrateSagaWithShipmentFailure()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("❌ CENÁRIO 3: Saga com Falha no Envio");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var orchestrator = new OrderSagaOrchestrator(shipmentShouldFail: true);
        var context = CreateSampleContext();

        var result = await orchestrator.ExecuteAsync(context);

        PrintResult(result, context);
    }

    static OrderSagaContext CreateSampleContext()
    {
        return new OrderSagaContext
        {
            CustomerId = Guid.NewGuid(),
            Items =
            [
                new OrderItem 
                { 
                    ProductId = Guid.NewGuid(), 
                    ProductName = "Notebook", 
                    Quantity = 1, 
                    UnitPrice = 3500.00m 
                },
                new OrderItem 
                { 
                    ProductId = Guid.NewGuid(), 
                    ProductName = "Mouse", 
                    Quantity = 2, 
                    UnitPrice = 150.00m 
                }
            ],
            TotalAmount = 3800.00m
        };
    }

    static void PrintResult(Core.SagaResult result, OrderSagaContext context)
    {
        Console.WriteLine($"\n  Resultado: {(result.IsSuccess ? "SUCESSO" : "FALHA")}");
        Console.WriteLine($"  Estado Final: {result.FinalState}");

        if (result.ExecutedSteps.Any())
        {
            Console.WriteLine($"  Passos Executados: {string.Join(" → ", result.ExecutedSteps)}");
        }

        if (result.CompensatedSteps.Any())
        {
            Console.WriteLine($"  Passos Compensados: {string.Join(" → ", result.CompensatedSteps)}");
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
            Console.WriteLine($"  Erro: {result.Error}");
        }

        if (result.IsSuccess)
        {
            Console.WriteLine($"\n  📦 Pedido: {context.OrderId}");
            Console.WriteLine($"  💳 Transação: {context.PaymentTransactionId}");
            Console.WriteLine($"  🚚 Rastreio: {context.ShippingTrackingCode}");
        }
    }
}
