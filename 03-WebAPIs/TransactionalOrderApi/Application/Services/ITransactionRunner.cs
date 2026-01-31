using TransactionalOrderApi.Infrastructure.Persistence;

namespace TransactionalOrderApi.Application.Services;

public interface ITransactionRunner
{
    Task ExecuteAsync(Func<AppDbContext, CancellationToken, Task> action, CancellationToken cancellationToken = default);
    Task<T> ExecuteAsync<T>(Func<AppDbContext, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}
