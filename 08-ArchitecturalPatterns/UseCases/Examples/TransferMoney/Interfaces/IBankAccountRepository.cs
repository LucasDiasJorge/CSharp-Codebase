using UseCases.Examples.TransferMoney.Entities;

namespace UseCases.Examples.TransferMoney.Interfaces;

/// <summary>
/// Repositório de contas bancárias
/// </summary>
public interface IBankAccountRepository
{
    Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(BankAccount account, CancellationToken cancellationToken = default);
}
