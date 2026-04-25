using Microsoft.EntityFrameworkCore;
using MoneyStorageApi.Data;
using MoneyStorageApi.Domain;

namespace MoneyStorageApi.Services;

public class AccountService
{
    private readonly MoneyStorageContext _context;

    public AccountService(MoneyStorageContext context)
    {
        _context = context;
    }

    public async Task<Account> CreateAccountAsync(string ownerName, decimal initialBalance, CancellationToken cancellationToken)
    {
        var account = new Account(ownerName, initialBalance);
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<Account?> GetAccountAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Accounts
            .Include(a => a.Movements)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<List<Account>> GetAccountsAsync(CancellationToken cancellationToken) =>
        await _context.Accounts
            .Include(a => a.Movements)
            .AsSplitQuery()
            .OrderBy(a => a.OwnerName)
            .ToListAsync(cancellationToken);

    public async Task<Account?> DepositAsync(Guid id, decimal amount, string? description, CancellationToken cancellationToken)
    {
        var account = await GetAccountTrackedAsync(id, cancellationToken);
        if (account is null)
            return null;

        account.Deposit(amount, description);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<Account?> WithdrawAsync(Guid id, decimal amount, string? description, CancellationToken cancellationToken)
    {
        var account = await GetAccountTrackedAsync(id, cancellationToken);
        if (account is null)
            return null;

        account.Withdraw(amount, description);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    private async Task<Account?> GetAccountTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Accounts
            .Include(a => a.Movements)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
}
