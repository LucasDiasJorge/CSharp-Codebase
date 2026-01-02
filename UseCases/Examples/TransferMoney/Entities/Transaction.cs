namespace UseCases.Examples.TransferMoney.Entities;

/// <summary>
/// Entidade: Transação
/// </summary>
public class Transaction
{
    public Guid Id { get; private set; }
    public Guid SourceAccountId { get; private set; }
    public Guid DestinationAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public TransactionStatus Status { get; private set; }

    public static Transaction Create(Guid sourceId, Guid destinationId, decimal amount, string description)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceId,
            DestinationAccountId = destinationId,
            Amount = amount,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        };
    }
}
