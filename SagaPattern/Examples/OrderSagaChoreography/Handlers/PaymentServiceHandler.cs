using SagaPattern.Examples.OrderSaga.Context;
using SagaPattern.Examples.OrderSagaChoreography.Events;
using SagaPattern.Examples.OrderSagaChoreography.Messaging;

namespace SagaPattern.Examples.OrderSagaChoreography.Handlers;

public sealed class PaymentServiceHandler
{
    private readonly IEventBus _bus;
    private readonly List<string> _executedSteps;
    private readonly List<string> _compensatedSteps;
    private readonly bool _shouldFail;

    public PaymentServiceHandler(
        IEventBus bus,
        List<string> executedSteps,
        List<string> compensatedSteps,
        bool shouldFail = false)
    {
        _bus = bus;
        _executedSteps = executedSteps;
        _compensatedSteps = compensatedSteps;
        _shouldFail = shouldFail;

        _bus.Subscribe<StockReserved>(OnStockReservedAsync);
        _bus.Subscribe<SagaFailed>(OnSagaFailedAsync);
    }

    private async Task OnStockReservedAsync(StockReserved evt, CancellationToken ct)
    {
        OrderSagaContext context = evt.Context;

        if (_shouldFail)
        {
            Console.WriteLine("    → [Payment] Pagamento recusado!");
            await _bus.PublishAsync(new SagaFailed(context, "ProcessPayment", "Pagamento recusado"), ct);
            return;
        }

        context.PaymentTransactionId = Guid.NewGuid().ToString();
        context.PaymentProcessed = true;
        _executedSteps.Add("ProcessPayment");
        Console.WriteLine($"    → [Payment] Pagamento processado: {context.PaymentTransactionId}");

        await _bus.PublishAsync(new PaymentProcessed(context), ct);
    }

    private Task OnSagaFailedAsync(SagaFailed evt, CancellationToken ct)
    {
        OrderSagaContext context = evt.Context;

        if (!context.PaymentProcessed)
        {
            return Task.CompletedTask;
        }

        Console.WriteLine($"    ← [Payment] Pagamento estornado: {context.PaymentTransactionId}");
        context.PaymentTransactionId = null;
        context.PaymentProcessed = false;
        _compensatedSteps.Add("ProcessPayment");
        return Task.CompletedTask;
    }
}
