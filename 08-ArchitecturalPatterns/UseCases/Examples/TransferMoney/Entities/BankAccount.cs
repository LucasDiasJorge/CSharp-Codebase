using UseCases.Core;

namespace UseCases.Examples.TransferMoney.Entities;

/// <summary>
/// Entidade de domínio: Conta Bancária
/// </summary>
public class BankAccount
{
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string HolderName { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }
    public decimal DailyTransferLimit { get; private set; }
    public decimal TransferredToday { get; private set; }

    private BankAccount() { }

    public static BankAccount Create(string accountNumber, string holderName, decimal initialBalance, decimal dailyLimit = 10000)
    {
        return new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            HolderName = holderName,
            Balance = initialBalance,
            IsActive = true,
            DailyTransferLimit = dailyLimit,
            TransferredToday = 0
        };
    }

    public Result Withdraw(decimal amount)
    {
        if (!IsActive)
            return Result.Failure("Conta inativa");

        if (amount <= 0)
            return Result.Failure("Valor deve ser positivo");

        if (amount > Balance)
            return Result.Failure("Saldo insuficiente");

        if (TransferredToday + amount > DailyTransferLimit)
            return Result.Failure($"Limite diário de transferência excedido. Disponível: {DailyTransferLimit - TransferredToday:C}");

        Balance -= amount;
        TransferredToday += amount;
        return Result.Success();
    }

    public Result Deposit(decimal amount)
    {
        if (!IsActive)
            return Result.Failure("Conta inativa");

        if (amount <= 0)
            return Result.Failure("Valor deve ser positivo");

        Balance += amount;
        return Result.Success();
    }
}
