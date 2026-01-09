using SagaPattern.Examples.OrderSaga;
using SagaPattern.Examples.OrderSaga.Context;
using SagaPattern.Examples.OrderSagaChoreography;

namespace SagaPattern;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              SAGA PATTERN EM C#                           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");

        var mode = ParseMode(args);

        if (mode is DemoMode.All or DemoMode.Orchestration)
        {
            Console.WriteLine("\n================== ORCHESTRATION ==================");
            await DemonstrateOrchestrationSagaSuccess();
            await DemonstrateOrchestrationSagaWithPaymentFailure();
            await DemonstrateOrchestrationSagaWithShipmentFailure();
        }

        if (mode is DemoMode.All or DemoMode.Choreography)
        {
            Console.WriteLine("\n================== CHOREOGRAPHY ===================");
            await DemonstrateChoreographySagaSuccess();
            await DemonstrateChoreographySagaWithPaymentFailure();
            await DemonstrateChoreographySagaWithShipmentFailure();
        }

        Console.WriteLine("\n✅ Demonstrações concluídas!");
    }

    static DemoMode ParseMode(string[] args)
    {
        if (args.Length == 0) return DemoMode.All;

        return args[0].Trim().ToLowerInvariant() switch
        {
            "orchestration" or "orchestrator" or "orq" => DemoMode.Orchestration,
            "choreography" or "choreograph" or "coreografia" or "choreo" => DemoMode.Choreography,
            "all" => DemoMode.All,
            _ => DemoMode.All
        };
    }

    static async Task DemonstrateOrchestrationSagaSuccess()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("✅ CENÁRIO 1: Saga com Sucesso");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var orchestrator = new OrderSagaOrchestrator();
        var context = CreateSampleContext();

        var result = await orchestrator.ExecuteAsync(context);

        PrintResult(result, context);
    }

    static async Task DemonstrateOrchestrationSagaWithPaymentFailure()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("❌ CENÁRIO 2: Saga com Falha no Pagamento");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var orchestrator = new OrderSagaOrchestrator(paymentShouldFail: true);
        var context = CreateSampleContext();

        var result = await orchestrator.ExecuteAsync(context);

        PrintResult(result, context);
    }

    static async Task DemonstrateOrchestrationSagaWithShipmentFailure()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("❌ CENÁRIO 3: Saga com Falha no Envio");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var orchestrator = new OrderSagaOrchestrator(shipmentShouldFail: true);
        var context = CreateSampleContext();

        var result = await orchestrator.ExecuteAsync(context);

        PrintResult(result, context);
    }

    static async Task DemonstrateChoreographySagaSuccess()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("✅ CENÁRIO 1: Saga com Sucesso");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var runner = new OrderSagaChoreographyRunner();
        var context = CreateSampleContext();

        var result = await runner.ExecuteAsync(context);

        PrintResult(result, context);
    }

    static async Task DemonstrateChoreographySagaWithPaymentFailure()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("❌ CENÁRIO 2: Saga com Falha no Pagamento");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var runner = new OrderSagaChoreographyRunner();
        var context = CreateSampleContext();

        var result = await runner.ExecuteAsync(context, paymentShouldFail: true);

        PrintResult(result, context);
    }

    static async Task DemonstrateChoreographySagaWithShipmentFailure()
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("❌ CENÁRIO 3: Saga com Falha no Envio");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var runner = new OrderSagaChoreographyRunner();
        var context = CreateSampleContext();

        var result = await runner.ExecuteAsync(context, shipmentShouldFail: true);

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

    enum DemoMode
    {
        All,
        Orchestration,
        Choreography
    }
}
