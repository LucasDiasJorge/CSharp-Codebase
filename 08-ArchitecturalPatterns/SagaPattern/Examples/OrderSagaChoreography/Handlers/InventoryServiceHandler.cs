using SagaPattern.Examples.OrderSaga.Context;
using SagaPattern.Examples.OrderSagaChoreography.Events;
using SagaPattern.Examples.OrderSagaChoreography.Messaging;

namespace SagaPattern.Examples.OrderSagaChoreography.Handlers;

public sealed class InventoryServiceHandler
{
    private readonly IEventBus _bus;
    private readonly List<string> _executedSteps;
    private readonly List<string> _compensatedSteps;
    private readonly bool _shouldFail;

    public InventoryServiceHandler(
        IEventBus bus,
        List<string> executedSteps,
        List<string> compensatedSteps,
        bool shouldFail = false)
    {
        _bus = bus;
        _executedSteps = executedSteps;
        _compensatedSteps = compensatedSteps;
        _shouldFail = shouldFail;

        _bus.Subscribe<OrderCreated>(OnOrderCreatedAsync);
        _bus.Subscribe<SagaFailed>(OnSagaFailedAsync);
    }

    private async Task OnOrderCreatedAsync(OrderCreated evt, CancellationToken ct)
    {
        OrderSagaContext context = evt.Context;

        if (_shouldFail)
        {
            Console.WriteLine("    → [Inventory] Estoque insuficiente!");
            await _bus.PublishAsync(new SagaFailed(context, "ReserveStock", "Estoque insuficiente"), ct);
            return;
        }

        context.StockReserved = true;
        _executedSteps.Add("ReserveStock");
        Console.WriteLine($"    → [Inventory] Estoque reservado para {context.Items.Count} itens");

        await _bus.PublishAsync(new StockReserved(context), ct);
    }

    private Task OnSagaFailedAsync(SagaFailed evt, CancellationToken ct)
    {
        OrderSagaContext context = evt.Context;

        if (!context.StockReserved)
        {
            return Task.CompletedTask;
        }

        context.StockReserved = false;
        _compensatedSteps.Add("ReserveStock");
        Console.WriteLine($"    ← [Inventory] Estoque liberado para {context.Items.Count} itens");
        return Task.CompletedTask;
    }
}
