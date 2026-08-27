using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Emissão de boleto bancário: compensação em D+1, sem autorização online.
/// </summary>
public sealed class BoletoResolver : IPaymentResolver
{
    private readonly ILogger<BoletoResolver> _logger;

    public BoletoResolver(ILogger<BoletoResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 100;

    public bool AppliesTo(PaymentRequest request)
    {
        return string.Equals(request.Method, PaymentMethods.Boleto, StringComparison.OrdinalIgnoreCase);
    }

    public async Task ProcessAsync(IReadOnlyCollection<PaymentRequest> requests, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gerando remessa de {RequestCount} boleto(s).", requests.Count);

        foreach (PaymentRequest request in requests)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(40), cancellationToken);

            _logger.LogInformation(
                "Boleto emitido para o pedido {OrderId}: valor {Amount}, vencimento {DueDate:dd/MM/yyyy}.",
                request.OrderId,
                request.Amount,
                DateTime.UtcNow.AddDays(3));
        }
    }
}
