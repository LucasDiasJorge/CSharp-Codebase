using PersistencePatterns.Core;

namespace PersistencePatterns.Examples.UnitOfWork.Interfaces;

/// <summary>
/// Unit of Work específico para operações de pedidos
/// Agrupa múltiplos repositórios em uma única transação
/// </summary>
public interface IOrderUnitOfWork : IUnitOfWork
{
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }
}
