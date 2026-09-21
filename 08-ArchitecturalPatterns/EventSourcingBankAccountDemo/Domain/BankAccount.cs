using EventSourcingBankAccountDemo.Events;

namespace EventSourcingBankAccountDemo.Domain;

/// <summary>
/// O agregado. O estado não é armazenado: é **derivado** da sequência de eventos.
/// Cada comando valida a regra contra o estado atual e, se passar, produz um evento
/// novo — nunca altera um campo diretamente de fora.
/// </summary>
public sealed class BankAccount
{
    private readonly List<AccountEvent> _uncommitted = new List<AccountEvent>();

    private BankAccount()
    {
        AccountId = string.Empty;
        Owner = string.Empty;
    }

    public string AccountId { get; private set; }

    public string Owner { get; private set; }

    public decimal Balance { get; private set; }

    public bool IsFrozen { get; private set; }

    /// <summary>Versão do último evento aplicado. É o que sustenta a concorrência otimista.</summary>
    public long Version { get; private set; }

    public IReadOnlyList<AccountEvent> UncommittedEvents => _uncommitted;

    public static BankAccount Open(string accountId, string owner, decimal initialDeposit)
    {
        if (initialDeposit < 0)
        {
            throw new InvalidOperationException("Deposito inicial nao pode ser negativo.");
        }

        BankAccount account = new BankAccount();
        account.Raise(new AccountOpened(accountId, owner, initialDeposit));

        return account;
    }

    /// <summary>
    /// Reconstrói o agregado aplicando os eventos em ordem. É a operação central do
    /// event sourcing: o estado é um fold sobre o fluxo.
    /// </summary>
    public static BankAccount Rehydrate(IEnumerable<AccountEvent> stream, AccountSnapshot? snapshot = null)
    {
        BankAccount account = new BankAccount();

        if (snapshot is not null)
        {
            // O snapshot e apenas um atalho: mesmo estado que se obteria aplicando
            // todos os eventos ate aquela versao. Nao e fonte da verdade.
            account.AccountId = snapshot.AccountId;
            account.Owner = snapshot.Owner;
            account.Balance = snapshot.Balance;
            account.IsFrozen = snapshot.IsFrozen;
            account.Version = snapshot.Version;
        }

        foreach (AccountEvent accountEvent in stream)
        {
            account.Apply(accountEvent);
            account.Version = accountEvent.Version;
        }

        return account;
    }

    public void Deposit(decimal amount)
    {
        EnsureNotFrozen();

        if (amount <= 0)
        {
            throw new InvalidOperationException("Deposito precisa ser positivo.");
        }

        Raise(new MoneyDeposited(amount));
    }

    public void Withdraw(decimal amount)
    {
        EnsureNotFrozen();

        if (amount <= 0)
        {
            throw new InvalidOperationException("Saque precisa ser positivo.");
        }

        // A regra e validada contra o estado DERIVADO dos eventos — nao contra uma
        // coluna de saldo que alguem poderia ter atualizado por fora.
        if (amount > Balance)
        {
            throw new InvalidOperationException($"Saldo insuficiente: saldo {Balance:F2}, pedido {amount:F2}.");
        }

        Raise(new MoneyWithdrawn(amount));
    }

    public void Freeze(string reason)
    {
        if (IsFrozen)
        {
            return;
        }

        Raise(new AccountFrozen(reason));
    }

    public void Unfreeze()
    {
        if (!IsFrozen)
        {
            return;
        }

        Raise(new AccountUnfrozen());
    }

    public AccountSnapshot TakeSnapshot()
    {
        return new AccountSnapshot(AccountId, Owner, Balance, IsFrozen, Version);
    }

    public void MarkCommitted()
    {
        _uncommitted.Clear();
    }

    /// <summary>Registra o evento como pendente e já aplica o efeito ao estado.</summary>
    private void Raise(AccountEvent accountEvent)
    {
        AccountEvent versioned = accountEvent with { Version = Version + 1 };

        Apply(versioned);
        Version = versioned.Version;
        _uncommitted.Add(versioned);
    }

    /// <summary>
    /// A transição de estado. Note que ela **não valida nada**: eventos do passado já
    /// aconteceram e precisam ser aplicáveis mesmo que a regra de hoje seja outra.
    /// Validar aqui quebraria a releitura de streams antigos.
    /// </summary>
    private void Apply(AccountEvent accountEvent)
    {
        switch (accountEvent)
        {
            case AccountOpened opened:
                AccountId = opened.AccountId;
                Owner = opened.Owner;
                Balance = opened.InitialDeposit;
                break;

            case MoneyDeposited deposited:
                Balance += deposited.Amount;
                break;

            case MoneyWithdrawn withdrawn:
                Balance -= withdrawn.Amount;
                break;

            case AccountFrozen:
                IsFrozen = true;
                break;

            case AccountUnfrozen:
                IsFrozen = false;
                break;
        }
    }

    private void EnsureNotFrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("Conta congelada nao aceita movimentacao.");
        }
    }
}

/// <summary>Estado materializado em uma versão, para evitar reler o fluxo inteiro.</summary>
public sealed record AccountSnapshot(string AccountId, string Owner, decimal Balance, bool IsFrozen, long Version);
