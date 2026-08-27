using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Rede de segurança do padrão: um método de pagamento sem resolver vira registro de auditoria
/// e pedido recusado, nunca uma <c>NullReferenceException</c> no orquestrador.
/// <b>Fica fora da coleção de candidatos</b> — o auto-registro o exclui de propósito e o serviço
/// o recebe pelo tipo concreto, para que seu <c>AppliesTo</c> sempre verdadeiro não engula
/// os demais resolvers.
/// </summary>
public sealed class UnsupportedPaymentResolver : IPaymentResolver
{
    private readonly ILogger<UnsupportedPaymentResolver> _logger;

    public UnsupportedPaymentResolver(ILogger<UnsupportedPaymentResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Último recurso: nunca participa da ordenação por prioridade.</summary>
    public int Priority => int.MaxValue;

    public bool AppliesTo(PaymentRequest request) => true;

    public Task ProcessAsync(IReadOnlyCollection<PaymentRequest> requests, CancellationToken cancellationToken)
    {
        foreach (PaymentRequest request in requests)
        {
            _logger.LogWarning(
                "Pedido {OrderId} recusado: nenhum resolver registrado para o método {Method}.",
                request.OrderId,
                request.Method);
        }

        return Task.CompletedTask;
    }
}
