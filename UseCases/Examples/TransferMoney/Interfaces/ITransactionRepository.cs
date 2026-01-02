using UseCases.Examples.TransferMoney.Entities;

namespace UseCases.Examples.TransferMoney.Interfaces;

/// <summary>
/// Repositório de transações
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
