using StrategyResolver.Domain;

namespace StrategyResolver.Services;

/// <summary>
/// Porta de entrada do fluxo de pagamento. O chamador entrega os pedidos e não escolhe
/// algoritmo algum: a resolução acontece internamente, a partir do próprio dado.
/// </summary>
public interface IPaymentProcessingService
{
    Task ProcessAsync(IEnumerable<PaymentRequest> requests, CancellationToken cancellationToken);
}
