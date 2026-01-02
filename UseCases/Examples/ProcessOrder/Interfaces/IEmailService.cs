namespace UseCases.Examples.ProcessOrder.Interfaces;

/// <summary>
/// Serviço de email
/// </summary>
public interface IEmailService
{
    Task SendOrderConfirmationAsync(string email, string orderNumber, decimal amount, CancellationToken cancellationToken = default);
}
