using PersistencePatterns.Examples.IdentityMap.Interfaces;

namespace PersistencePatterns.Examples.IdentityMap.Implementations;

/// <summary>
/// Implementação genérica do Identity Map
/// Mantém cache de entidades para evitar duplicação em memória
/// </summary>
public class IdentityMap<TEntity, TId> : IIdentityMap<TEntity, TId> 
    where TEntity : class
    where TId : notnull
{
    private readonly Dictionary<TId, TEntity> _entities = [];

    public TEntity? Get(TId id)
    {
        _entities.TryGetValue(id, out var entity);
        return entity;
    }

    public void Add(TId id, TEntity entity)
    {
        _entities[id] = entity;
    }

    public void Remove(TId id)
    {
        _entities.Remove(id);
    }

    public void Clear()
    {
        _entities.Clear();
    }

    public bool Contains(TId id)
    {
        return _entities.ContainsKey(id);
    }

    public int Count => _entities.Count;
}
