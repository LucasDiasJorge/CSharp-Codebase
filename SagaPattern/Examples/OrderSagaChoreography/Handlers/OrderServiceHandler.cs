using SagaPattern.Examples.OrderSaga.Context;
using SagaPattern.Examples.OrderSagaChoreography.Events;
using SagaPattern.Examples.OrderSagaChoreography.Messaging;

namespace SagaPattern.Examples.OrderSagaChoreography.Handlers;

public sealed class OrderServiceHandler
{
    private readonly IEventBus _bus;
    private readonly List<string> _executedSteps;
    private readonly List<string> _compensatedSteps;

    public OrderServiceHandler(IEventBus bus, List<string> executedSteps, List<string> compensatedSteps)
    {
        _bus = bus;
        _executedSteps = executedSteps;
        _compensatedSteps = compensatedSteps;

        _bus.Subscribe<StartOrderSaga>(OnStartAsync);
        _bus.Subscribe<SagaFailed>(OnSagaFailedAsync);
    }

    private async Task OnStartAsync(StartOrderSaga evt, CancellationToken ct)
    {
        OrderSagaContext context = evt.Context;

        context.OrderId = Guid.NewGuid();
        context.OrderCreated = true;

        _executedSteps.Add("CreateOrder");
        Console.WriteLine($"    → [Order] Pedido {context.OrderId} criado");

        await _bus.PublishAsync(new OrderCreated(context), ct);
    }

    private Task OnSagaFailedAsync(SagaFailed evt, CancellationToken ct)
    {
        OrderSagaContext context = evt.Context;

        if (!context.OrderCreated)
        {
            return Task.CompletedTask;
        }

        context.OrderCreated = false;
        _compensatedSteps.Add("CreateOrder");
        Console.WriteLine($"    ← [Order] Pedido {context.OrderId} cancelado");
        return Task.CompletedTask;
    }
}
