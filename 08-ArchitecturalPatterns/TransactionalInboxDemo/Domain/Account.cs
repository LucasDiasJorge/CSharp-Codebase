namespace TransactionalInboxDemo.Domain;

/// <summary>
/// Entidade de negócio. O saldo é o que denuncia um processamento duplicado: creditar
/// duas vezes a mesma mensagem deixa um número errado que ninguém percebe na hora.
/// </summary>
public sealed class Account
{
    public Account(string accountId, decimal balance)
    {
        AccountId = accountId;
        Balance = balance;
    }

    private Account()
    {
        AccountId = string.Empty;
    }

    public int Id { get; private set; }

    public string AccountId { get; private set; }

    public decimal Balance { get; private set; }

    /// <summary>Quantas vezes o crédito foi de fato aplicado — o contador da duplicata.</summary>
    public int AppliedCredits { get; private set; }

    public void Credit(decimal amount)
    {
        Balance += amount;
        AppliedCredits++;
    }
}
