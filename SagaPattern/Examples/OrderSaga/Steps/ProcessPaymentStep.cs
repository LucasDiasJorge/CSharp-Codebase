using SagaPattern.Core;
using SagaPattern.Examples.OrderSaga.Context;

namespace SagaPattern.Examples.OrderSaga.Steps;

/// <summary>
/// Passo 3: Processar pagamento
/// </summary>
public class ProcessPaymentStep : ISagaStep<OrderSagaContext>
{
    private readonly bool _shouldFail;

    public ProcessPaymentStep(bool shouldFail = false)
    {
        _shouldFail = shouldFail;
    }

    public string Name => "ProcessPayment";

    public Task<bool> ExecuteAsync(OrderSagaContext context, CancellationToken ct = default)
    {
        if (_shouldFail)
        {
            Console.WriteLine("    → Pagamento recusado!");
            return Task.FromResult(false);
        }

        // Simula processamento de pagamento
        context.PaymentTransactionId = Guid.NewGuid().ToString();
        context.PaymentProcessed = true;
        Console.WriteLine($"    → Pagamento processado: {context.PaymentTransactionId}");
        return Task.FromResult(true);
    }

    public Task CompensateAsync(OrderSagaContext context, CancellationToken ct = default)
    {
        // Simula estorno
        Console.WriteLine($"    ← Pagamento estornado: {context.PaymentTransactionId}");
        context.PaymentTransactionId = null;
        context.PaymentProcessed = false;
        return Task.CompletedTask;
    }
}
