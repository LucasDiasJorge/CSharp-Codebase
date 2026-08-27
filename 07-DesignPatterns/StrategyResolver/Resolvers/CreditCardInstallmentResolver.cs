using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Cartão de crédito parcelado: além da autorização, calcula juros e monta o plano de parcelas.
/// É o caso que justifica <c>Priority</c> no contrato — dois resolvers aceitam CREDIT_CARD e a
/// desambiguação precisa ser explícita, não acidental.
/// </summary>
public sealed class CreditCardInstallmentResolver : IPaymentResolver
{
    private const decimal MonthlyInterestRate = 0.0199m;

    private readonly ILogger<CreditCardInstallmentResolver> _logger;

    public CreditCardInstallmentResolver(ILogger<CreditCardInstallmentResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 100;

    /// <summary>
    /// Critério composto: método <b>e</b> número de parcelas. É o tipo de regra que não cabe
    /// num switch sobre uma única string.
    /// </summary>
    public bool AppliesTo(PaymentRequest request)
    {
        return string.Equals(request.Method, PaymentMethods.CreditCard, StringComparison.OrdinalIgnoreCase)
            && request.Installments > 1;
    }

    public async Task ProcessAsync(IReadOnlyCollection<PaymentRequest> requests, CancellationToken cancellationToken)
    {
        foreach (PaymentRequest request in requests)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(40), cancellationToken);

            decimal total = request.Amount * (1 + (MonthlyInterestRate * request.Installments));
            decimal installmentAmount = Math.Round(total / request.Installments, 2);

            _logger.LogInformation(
                "Cartão parcelado autorizado para o pedido {OrderId}: {Installments}x de {InstallmentAmount} (total {Total}).",
                request.OrderId,
                request.Installments,
                installmentAmount,
                Math.Round(total, 2));
        }
    }
}
