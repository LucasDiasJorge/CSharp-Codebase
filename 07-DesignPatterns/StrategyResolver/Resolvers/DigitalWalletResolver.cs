using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Carteira digital. Representa o método adicionado depois que a plataforma já estava em
/// produção: com o auto-registro por scanning, basta a classe existir no assembly para o
/// container passar a entregá-la — nenhuma linha do orquestrador muda.
/// </summary>
public sealed class DigitalWalletResolver : IPaymentResolver
{
    private readonly ILogger<DigitalWalletResolver> _logger;

    public DigitalWalletResolver(ILogger<DigitalWalletResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 100;

    public bool AppliesTo(PaymentRequest request)
    {
        return string.Equals(request.Method, PaymentMethods.DigitalWallet, StringComparison.OrdinalIgnoreCase);
    }

    public async Task ProcessAsync(IReadOnlyCollection<PaymentRequest> requests, CancellationToken cancellationToken)
    {
        foreach (PaymentRequest request in requests)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(40), cancellationToken);

            _logger.LogInformation(
                "Carteira digital debitada para o pedido {OrderId}: valor {Amount}, saldo confirmado pelo provedor.",
                request.OrderId,
                request.Amount);
        }
    }
}
