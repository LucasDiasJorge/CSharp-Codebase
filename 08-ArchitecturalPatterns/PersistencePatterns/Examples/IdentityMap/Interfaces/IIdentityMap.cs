namespace PersistencePatterns.Examples.IdentityMap.Interfaces;

/// <summary>
/// Interface do Identity Map
/// Garante uma única instância de cada entidade em memória
/// </summary>
public interface IIdentityMap<TEntity, TId> where TEntity : class
{
    TEntity? Get(TId id);
    void Add(TId id, TEntity entity);
    void Remove(TId id);
    void Clear();
    bool Contains(TId id);
}
