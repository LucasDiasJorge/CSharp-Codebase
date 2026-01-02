using UseCases.Examples.ProcessOrder.DTOs;

namespace UseCases.Examples.ProcessOrder.Interfaces;

/// <summary>
/// Gateway de pagamento
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, PaymentMethod method, CancellationToken cancellationToken = default);
}

public record PaymentResult(bool Success, string TransactionId, string? ErrorMessage);
