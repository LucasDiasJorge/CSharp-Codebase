namespace UseCases.Examples.TransferMoney.DTOs;

/// <summary>
/// DTO de entrada para transferência
/// </summary>
public record TransferMoneyInput(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Description
);
