using SagaPattern.Core;
using SagaPattern.Examples.OrderSaga.Context;
using SagaPattern.Examples.OrderSaga.Steps;

namespace SagaPattern.Examples.OrderSaga;

/// <summary>
/// Orquestrador da Saga de Pedido
/// Coordena os passos: Criar Pedido → Reservar Estoque → Pagamento → Envio
/// </summary>
public class OrderSagaOrchestrator : SagaOrchestrator<OrderSagaContext>
{
    public OrderSagaOrchestrator(
        bool stockShouldFail = false,
        bool paymentShouldFail = false,
        bool shipmentShouldFail = false)
    {
        // Definir passos na ordem de execução
        AddStep(new CreateOrderStep());
        AddStep(new ReserveStockStep(stockShouldFail));
        AddStep(new ProcessPaymentStep(paymentShouldFail));
        AddStep(new CreateShipmentStep(shipmentShouldFail));
    }
}
