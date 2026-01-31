using SagaPattern.Core;
using SagaPattern.Examples.OrderSaga.Context;
using SagaPattern.Examples.OrderSagaChoreography.Events;
using SagaPattern.Examples.OrderSagaChoreography.Handlers;
using SagaPattern.Examples.OrderSagaChoreography.Messaging;

namespace SagaPattern.Examples.OrderSagaChoreography;

public sealed class OrderSagaChoreographyRunner
{
    public async Task<SagaResult> ExecuteAsync(
        OrderSagaContext context,
        bool stockShouldFail = false,
        bool paymentShouldFail = false,
        bool shipmentShouldFail = false,
        CancellationToken ct = default)
    {
        InMemoryEventBus bus = new InMemoryEventBus();
        List<string> executedSteps = new List<string>();
        List<string> compensatedSteps = new List<string>();

        _ = new OrderServiceHandler(bus, executedSteps, compensatedSteps);
        _ = new InventoryServiceHandler(bus, executedSteps, compensatedSteps, stockShouldFail);
        _ = new PaymentServiceHandler(bus, executedSteps, compensatedSteps, paymentShouldFail);
        _ = new ShippingServiceHandler(bus, executedSteps, compensatedSteps, shipmentShouldFail);

        TaskCompletionSource<SagaResult> tcs = new TaskCompletionSource<SagaResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Finalizers: subscribe AFTER handlers so this runs after compensations.
        bus.Subscribe<SagaCompleted>((evt, _) =>
        {
            tcs.TrySetResult(SagaResult.Success(executedSteps));
            return Task.CompletedTask;
        });

        bus.Subscribe<SagaFailed>((evt, _) =>
        {
            tcs.TrySetResult(SagaResult.Failure(evt.Error, executedSteps, compensatedSteps));
            return Task.CompletedTask;
        });

        Console.WriteLine("\n  [Choreography] Iniciando execução (event-driven)");
        await bus.PublishAsync(new StartOrderSaga(context), ct);

        return await tcs.Task;
    }
}
