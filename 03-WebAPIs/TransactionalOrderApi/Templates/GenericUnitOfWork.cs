using System.Data.Common;
using System.Data;
using System.Threading;

namespace TransactionalOrderApi.Templates;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<T> ExecuteInTransactionAsync<T>(Func<DbTransaction, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<DbTransaction, CancellationToken, Task> action, CancellationToken cancellationToken = default);
}

public sealed class GenericUnitOfWork(DbConnection connection) : IUnitOfWork
{
    private readonly DbConnection _connection = connection;

    public async Task<T> ExecuteInTransactionAsync<T>(Func<DbTransaction, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);

        await using DbTransaction transaction = await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            T result = await action(transaction, cancellationToken).ConfigureAwait(false);
            // Note: repositories should not call SaveChanges when using ADO.NET/Dapper; caller ensures persistence commit if needed
            transaction.Commit();
            return result;
        }
        catch
        {
            try { transaction.Rollback(); } catch { /* swallow rollback exceptions to preserve original */ }
            throw;
        }
    }

    public Task ExecuteInTransactionAsync(Func<DbTransaction, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        => ExecuteInTransactionAsync(async (tx, ct) => { await action(tx, ct).ConfigureAwait(false); return true; }, cancellationToken);

    private async Task EnsureOpenAsync(CancellationToken cancellationToken)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public ValueTask DisposeAsync()
    {
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
