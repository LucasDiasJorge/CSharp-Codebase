using SagaPattern.Examples.OrderSaga.Context;
using SagaPattern.Examples.OrderSagaChoreography.Events;
using SagaPattern.Examples.OrderSagaChoreography.Messaging;

namespace SagaPattern.Examples.OrderSagaChoreography.Handlers;

public sealed class ShippingServiceHandler
{
    private readonly IEventBus _bus;
    private readonly List<string> _executedSteps;
    private readonly List<string> _compensatedSteps;
    private readonly bool _shouldFail;

    public ShippingServiceHandler(
        IEventBus bus,
        List<string> executedSteps,
        List<string> compensatedSteps,
        bool shouldFail = false)
    {
        _bus = bus;
        _executedSteps = executedSteps;
        _compensatedSteps = compensatedSteps;
        _shouldFail = shouldFail;

        _bus.Subscribe<PaymentProcessed>(OnPaymentProcessedAsync);
        _bus.Subscribe<SagaFailed>(OnSagaFailedAsync);
    }

    private async Task OnPaymentProcessedAsync(PaymentProcessed evt, CancellationToken ct)
    {
        var context = evt.Context;

        if (_shouldFail)
        {
            Console.WriteLine("    → [Shipping] Falha ao criar envio!");
            await _bus.PublishAsync(new SagaFailed(context, "CreateShipment", "Falha ao criar envio"), ct);
            return;
        }

        context.ShippingTrackingCode = $"BR{Random.Shared.Next(100000, 999999)}";
        _executedSteps.Add("CreateShipment");
        Console.WriteLine($"    → [Shipping] Envio criado: {context.ShippingTrackingCode}");

        await _bus.PublishAsync(new ShipmentCreated(context), ct);
    }

    private Task OnSagaFailedAsync(SagaFailed evt, CancellationToken ct)
    {
        var context = evt.Context;

        if (string.IsNullOrWhiteSpace(context.ShippingTrackingCode))
        {
            return Task.CompletedTask;
        }

        Console.WriteLine($"    ← [Shipping] Envio cancelado: {context.ShippingTrackingCode}");
        context.ShippingTrackingCode = null;
        _compensatedSteps.Add("CreateShipment");
        return Task.CompletedTask;
    }
}
