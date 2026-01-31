namespace UseCases.Core;

/// <summary>
/// Unit of Work para garantir consistência transacional
/// </summary>
public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
