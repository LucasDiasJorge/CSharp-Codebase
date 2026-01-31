using System;

namespace MoneyStorageApi.Domain;

public enum MovementType
{
    Deposit = 1,
    Withdrawal = 2
}

public class MoneyMovement
{
    private MoneyMovement() { }

    private MoneyMovement(Guid accountId, MovementType type, decimal amount, string? description)
    {
        AccountId = accountId;
        Type = type;
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Description = description?.Trim() ?? string.Empty;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public Guid AccountId { get; private set; }
    public MovementType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime OccurredAtUtc { get; private set; }

    public static MoneyMovement CreateDeposit(Guid accountId, decimal amount, string? description) =>
        new(accountId, MovementType.Deposit, amount, description);

    public static MoneyMovement CreateWithdrawal(Guid accountId, decimal amount, string? description) =>
        new(accountId, MovementType.Withdrawal, amount, description);
}
