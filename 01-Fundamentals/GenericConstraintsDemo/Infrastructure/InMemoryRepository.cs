using System;
using System.Collections.Generic;
using System.Linq;
using GenericConstraintsDemo.Contracts;

namespace GenericConstraintsDemo.Infrastructure;

public sealed class InMemoryRepository<TEntity> : IReadableCatalog<TEntity>
    where TEntity : class, IEntity
{
    private readonly Dictionary<string, TEntity> items = new(StringComparer.OrdinalIgnoreCase);

    public void Add(TEntity entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Id))
        {
            throw new ArgumentException("Entidades genericas precisam de Id antes de serem armazenadas.");
        }

        items[entity.Id] = entity;
    }

    public TEntity? FindById(string id)
    {
        bool wasFound = items.TryGetValue(id, out TEntity? entity);
        return wasFound ? entity : null;
    }

    public IReadOnlyCollection<TEntity> GetAll()
    {
        TEntity[] entities = items.Values.ToArray();
        return entities;
    }
}
