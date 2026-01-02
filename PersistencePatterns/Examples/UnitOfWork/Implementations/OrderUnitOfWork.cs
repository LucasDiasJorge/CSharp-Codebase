using PersistencePatterns.Examples.UnitOfWork.Entities;
using PersistencePatterns.Examples.UnitOfWork.Interfaces;

namespace PersistencePatterns.Examples.UnitOfWork.Implementations;

/// <summary>
/// Implementação do Unit of Work
/// Coordena múltiplos repositórios em uma única transação
/// </summary>
public class OrderUnitOfWork : IOrderUnitOfWork
{
    private readonly List<Order> _orders = [];
    private readonly List<Payment> _payments = [];
    
    // Backup para rollback
    private List<Order>? _ordersBackup;
    private List<Payment>? _paymentsBackup;
    private bool _inTransaction;
    private bool _disposed;

    public IOrderRepository Orders { get; }
    public IPaymentRepository Payments { get; }

    public OrderUnitOfWork()
    {
        Orders = new InMemoryOrderRepository(_orders);
        Payments = new InMemoryPaymentRepository(_payments);
    }

    public Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_inTransaction)
            throw new InvalidOperationException("Transação já iniciada");

        // Criar backup para possível rollback
        _ordersBackup = _orders.ToList();
        _paymentsBackup = _payments.ToList();
        _inTransaction = true;

        Console.WriteLine("  [UoW] Transação iniciada");
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Em memória, as mudanças já estão aplicadas
        var changes = _orders.Count + _payments.Count;
        Console.WriteLine($"  [UoW] SaveChanges: {changes} entidades");
        return Task.FromResult(changes);
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        if (!_inTransaction)
            throw new InvalidOperationException("Nenhuma transação ativa");

        // Limpar backups - mudanças confirmadas
        _ordersBackup = null;
        _paymentsBackup = null;
        _inTransaction = false;

        Console.WriteLine("  [UoW] Transação COMMITTED");
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        if (!_inTransaction)
            throw new InvalidOperationException("Nenhuma transação ativa");

        // Restaurar estado anterior
        _orders.Clear();
        _orders.AddRange(_ordersBackup ?? []);

        _payments.Clear();
        _payments.AddRange(_paymentsBackup ?? []);

        _ordersBackup = null;
        _paymentsBackup = null;
        _inTransaction = false;

        Console.WriteLine("  [UoW] Transação ROLLED BACK");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_inTransaction)
        {
            RollbackAsync().Wait();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
