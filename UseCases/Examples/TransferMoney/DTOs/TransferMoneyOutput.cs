namespace UseCases.Examples.TransferMoney.DTOs;

/// <summary>
/// DTO de saída da transferência
/// </summary>
public record TransferMoneyOutput(
    Guid TransactionId,
    decimal SourceBalance,
    decimal DestinationBalance,
    DateTime TransferredAt
);
