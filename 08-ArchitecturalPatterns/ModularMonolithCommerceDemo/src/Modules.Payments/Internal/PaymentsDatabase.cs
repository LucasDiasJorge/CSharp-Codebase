using System.Collections.Concurrent;

namespace Modules.Payments.Internal;

/// <summary>Armazenamento próprio de Pagamentos. <c>internal</c>, como os demais.</summary>
internal sealed class PaymentsDatabase
{
    private readonly ConcurrentDictionary<string, PaymentEntity> _payments = new ConcurrentDictionary<string, PaymentEntity>();

    public void Save(PaymentEntity payment) => _payments[payment.OrderId] = payment;

    public IReadOnlyCollection<PaymentEntity> All() => _payments.Values.ToArray();
}

internal sealed class PaymentEntity
{
    public PaymentEntity(string orderId, decimal amount, string status, string gatewayReference)
    {
        OrderId = orderId;
        Amount = amount;
        Status = status;
        GatewayReference = gatewayReference;
    }

    public string OrderId { get; }

    public decimal Amount { get; }

    public string Status { get; }

    /// <summary>Detalhe do gateway: interno ao módulo, fora do contrato público.</summary>
    public string GatewayReference { get; }
}
