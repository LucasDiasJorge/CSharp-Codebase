namespace UseCases.Examples.TransferMoney.Interfaces;

/// <summary>
/// Serviço de auditoria
/// </summary>
public interface IAuditService
{
    Task LogTransferAsync(Guid transactionId, Guid sourceId, Guid destinationId, decimal amount, CancellationToken cancellationToken = default);
}
