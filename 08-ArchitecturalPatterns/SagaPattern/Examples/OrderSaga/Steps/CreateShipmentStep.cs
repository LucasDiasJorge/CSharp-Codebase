using SagaPattern.Core;
using SagaPattern.Examples.OrderSaga.Context;

namespace SagaPattern.Examples.OrderSaga.Steps;

/// <summary>
/// Passo 4: Criar ordem de envio
/// </summary>
public class CreateShipmentStep : ISagaStep<OrderSagaContext>
{
    private readonly bool _shouldFail;

    public CreateShipmentStep(bool shouldFail = false)
    {
        _shouldFail = shouldFail;
    }

    public string Name => "CreateShipment";

    public Task<bool> ExecuteAsync(OrderSagaContext context, CancellationToken ct = default)
    {
        if (_shouldFail)
        {
            Console.WriteLine("    → Falha ao criar envio!");
            return Task.FromResult(false);
        }

        // Simula criação de ordem de envio
        context.ShippingTrackingCode = $"BR{new Random().Next(100000, 999999)}";
        Console.WriteLine($"    → Envio criado: {context.ShippingTrackingCode}");
        return Task.FromResult(true);
    }

    public Task CompensateAsync(OrderSagaContext context, CancellationToken ct = default)
    {
        // Simula cancelamento do envio
        Console.WriteLine($"    ← Envio cancelado: {context.ShippingTrackingCode}");
        context.ShippingTrackingCode = null;
        return Task.CompletedTask;
    }
}
