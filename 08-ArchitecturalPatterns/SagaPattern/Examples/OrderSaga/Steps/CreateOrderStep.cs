using SagaPattern.Core;
using SagaPattern.Examples.OrderSaga.Context;

namespace SagaPattern.Examples.OrderSaga.Steps;

/// <summary>
/// Passo 1: Criar o pedido no sistema
/// </summary>
public class CreateOrderStep : ISagaStep<OrderSagaContext>
{
    public string Name => "CreateOrder";

    public Task<bool> ExecuteAsync(OrderSagaContext context, CancellationToken ct = default)
    {
        // Simula criação do pedido
        context.OrderId = Guid.NewGuid();
        context.OrderCreated = true;

        Console.WriteLine($"    → Pedido {context.OrderId} criado");
        return Task.FromResult(true);
    }

    public Task CompensateAsync(OrderSagaContext context, CancellationToken ct = default)
    {
        // Simula cancelamento do pedido
        context.OrderCreated = false;
        Console.WriteLine($"    ← Pedido {context.OrderId} cancelado");
        return Task.CompletedTask;
    }
}
