using System.Data;

namespace TransactionPattern.Core;

/// <summary>
/// Repositório base que implementa funcionalidades comuns de transação.
/// </summary>
public abstract class BaseRepository : IRepository
{
    protected readonly IDbConnection _connection;

    protected BaseRepository(IDbConnection connection)
    {
        _connection = connection;
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    public IDbTransaction BeginTransaction()
    {
        return _connection.BeginTransaction();
    }

    /// <summary>
    /// Método auxiliar que executa uma ação dentro de uma transação.
    /// Garante que a transação será commitada em caso de sucesso ou
    /// revertida em caso de exceção.
    /// </summary>
    /// <param name="action">Ação a ser executada dentro da transação.</param>
    protected async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action)
    {
        using IDbTransaction transaction = BeginTransaction();
        try
        {
            await action(transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Sobrecarga síncrona do método ExecuteInTransactionAsync.
    /// </summary>
    protected void ExecuteInTransaction(Action<IDbTransaction> action)
    {
        using IDbTransaction transaction = BeginTransaction();
        try
        {
            action(transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
