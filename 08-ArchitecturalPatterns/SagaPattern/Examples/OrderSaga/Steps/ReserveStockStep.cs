using SagaPattern.Core;
using SagaPattern.Examples.OrderSaga.Context;

namespace SagaPattern.Examples.OrderSaga.Steps;

/// <summary>
/// Passo 2: Reservar estoque dos produtos
/// </summary>
public class ReserveStockStep : ISagaStep<OrderSagaContext>
{
    private readonly bool _shouldFail;

    public ReserveStockStep(bool shouldFail = false)
    {
        _shouldFail = shouldFail;
    }

    public string Name => "ReserveStock";

    public Task<bool> ExecuteAsync(OrderSagaContext context, CancellationToken ct = default)
    {
        if (_shouldFail)
        {
            Console.WriteLine("    → Estoque insuficiente!");
            return Task.FromResult(false);
        }

        // Simula reserva de estoque
        context.StockReserved = true;
        Console.WriteLine($"    → Estoque reservado para {context.Items.Count} itens");
        return Task.FromResult(true);
    }

    public Task CompensateAsync(OrderSagaContext context, CancellationToken ct = default)
    {
        // Simula liberação de estoque
        context.StockReserved = false;
        Console.WriteLine($"    ← Estoque liberado para {context.Items.Count} itens");
        return Task.CompletedTask;
    }
}
