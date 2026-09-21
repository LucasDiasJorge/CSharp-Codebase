using Microsoft.Extensions.Logging;
using Modules.Abstractions.Contracts;
using Modules.Abstractions.Events;
using Modules.Payments.Internal;

namespace Modules.Payments;

/// <summary>
/// Módulo de Pagamentos. Não conhece nem Pedidos nem Catálogo: só reage ao evento
/// <see cref="OrderPlaced"/> e publica o resultado. A dependência é sobre o evento,
/// que mora nas abstrações compartilhadas — não sobre o módulo que o emitiu.
/// </summary>
public sealed class PaymentsModule : IPaymentsModule
{
    /// <summary>Acima disto, a cobrança é recusada — regra interna deste módulo.</summary>
    private const decimal AutoApprovalLimit = 2000m;

    private readonly PaymentsDatabase _database = new PaymentsDatabase();
    private readonly IEventBus _bus;
    private readonly ILogger<PaymentsModule> _logger;

    public PaymentsModule(IEventBus bus, ILogger<PaymentsModule> logger)
    {
        _bus = bus;
        _logger = logger;

        _bus.Subscribe<OrderPlaced>(OnOrderPlacedAsync);
    }

    public Task<IReadOnlyList<PaymentInfo>> ListAsync(CancellationToken cancellationToken)
    {
        List<PaymentInfo> payments = new List<PaymentInfo>();
        foreach (PaymentEntity payment in _database.All())
        {
            // GatewayReference nao entra no DTO: e detalhe interno.
            payments.Add(new PaymentInfo(payment.OrderId, payment.Amount, payment.Status));
        }

        return Task.FromResult<IReadOnlyList<PaymentInfo>>(payments);
    }

    private async Task OnOrderPlacedAsync(OrderPlaced orderPlaced, CancellationToken cancellationToken)
    {
        if (orderPlaced.Total > AutoApprovalLimit)
        {
            _database.Save(new PaymentEntity(orderPlaced.OrderId, orderPlaced.Total, "recusado", "n/d"));

            _logger.LogWarning(
                "Pagamentos: cobranca de {Total:F2} do pedido {Pedido} recusada (limite {Limite:F2}).",
                orderPlaced.Total,
                orderPlaced.OrderId,
                AutoApprovalLimit);

            await _bus.PublishAsync(new PaymentFailed(orderPlaced.OrderId, "valor acima do limite"), cancellationToken)
                .ConfigureAwait(false);

            return;
        }

        _database.Save(new PaymentEntity(orderPlaced.OrderId, orderPlaced.Total, "confirmado", $"GW-{Guid.NewGuid():N}"[..12]));

        _logger.LogInformation("Pagamentos: {Total:F2} do pedido {Pedido} confirmado.", orderPlaced.Total, orderPlaced.OrderId);

        await _bus.PublishAsync(new PaymentConfirmed(orderPlaced.OrderId, orderPlaced.Total), cancellationToken)
            .ConfigureAwait(false);
    }
}
