using System;
using System.Collections.Generic;

namespace MoneyStorageApi.Domain;

public class Account
{
    private Account() { }

    public Account(string ownerName, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
            throw new ArgumentException("Owner name is required.", nameof(ownerName));

        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative.", nameof(initialBalance));

        OwnerName = ownerName.Trim();

        if (initialBalance > 0)
            Deposit(initialBalance, "Initial balance");
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string OwnerName { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
    public List<MoneyMovement> Movements { get; private set; } = new();

    public void Deposit(decimal amount, string? description = null)
    {
        ValidateAmount(amount);
        Balance += amount;
        Movements.Add(MoneyMovement.CreateDeposit(Id, amount, description));
    }

    public void Withdraw(decimal amount, string? description = null)
    {
        ValidateAmount(amount);

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient funds for withdrawal.");

        Balance -= amount;
        Movements.Add(MoneyMovement.CreateWithdrawal(Id, amount, description));
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        if (decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Amount must contain at most two decimal places.", nameof(amount));
    }
}
