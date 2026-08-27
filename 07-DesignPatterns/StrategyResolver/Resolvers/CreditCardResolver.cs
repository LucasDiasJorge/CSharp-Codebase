using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Cartão de crédito à vista: autorização e captura no mesmo fluxo.
/// <b>Prioridade 200 de propósito.</b> O <see cref="CreditCardInstallmentResolver"/> aceita um
/// subconjunto do que este resolver aceita, então precisa ser avaliado antes; se os dois
/// tivessem a mesma prioridade, a seleção voltaria a depender da ordem de registro.
/// </summary>
public sealed class CreditCardResolver : IPaymentResolver
{
    private readonly ILogger<CreditCardResolver> _logger;

    public CreditCardResolver(ILogger<CreditCardResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 200;

    public bool AppliesTo(PaymentRequest request)
    {
        return string.Equals(request.Method, PaymentMethods.CreditCard, StringComparison.OrdinalIgnoreCase);
    }

    public async Task ProcessAsync(IReadOnlyCollection<PaymentRequest> requests, CancellationToken cancellationToken)
    {
        foreach (PaymentRequest request in requests)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(40), cancellationToken);

            _logger.LogInformation(
                "Cartão à vista autorizado e capturado para o pedido {OrderId}: valor {Amount}.",
                request.OrderId,
                request.Amount);
        }
    }
}
