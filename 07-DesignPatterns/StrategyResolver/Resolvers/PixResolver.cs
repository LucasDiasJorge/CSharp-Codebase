using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Liquidação instantânea via PIX. Nenhum outro resolver disputa este método,
/// então a prioridade padrão basta.
/// </summary>
public sealed class PixResolver : IPaymentResolver
{
    private readonly ILogger<PixResolver> _logger;

    public PixResolver(ILogger<PixResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 100;

    public bool AppliesTo(PaymentRequest request)
    {
        return string.Equals(request.Method, PaymentMethods.Pix, StringComparison.OrdinalIgnoreCase);
    }

    public async Task ProcessAsync(IReadOnlyCollection<PaymentRequest> requests, CancellationToken cancellationToken)
    {
        foreach (PaymentRequest request in requests)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(40), cancellationToken);

            _logger.LogInformation(
                "PIX liquidado para o pedido {OrderId}: valor {Amount}, e2eId {EndToEndId}.",
                request.OrderId,
                request.Amount,
                $"E{DateTime.UtcNow:yyyyMMddHHmmss}{request.OrderId}");
        }
    }
}
