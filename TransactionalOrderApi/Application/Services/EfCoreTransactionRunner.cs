using Microsoft.EntityFrameworkCore.Storage;
using TransactionalOrderApi.Infrastructure.Persistence;

namespace TransactionalOrderApi.Application.Services;

public class EfCoreTransactionRunner(AppDbContext context) : ITransactionRunner
{
    private readonly AppDbContext _context = context;

    public Task ExecuteAsync(Func<AppDbContext, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        => ExecuteInternalAsync(async (db, token) => { await action(db, token); return true; }, cancellationToken);

    public Task<T> ExecuteAsync<T>(Func<AppDbContext, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        => ExecuteInternalAsync(action, cancellationToken);

    private async Task<T> ExecuteInternalAsync<T>(Func<AppDbContext, CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            T result = await action(_context, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
