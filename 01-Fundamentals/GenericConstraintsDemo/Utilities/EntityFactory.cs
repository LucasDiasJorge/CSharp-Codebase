using System;
using GenericConstraintsDemo.Contracts;

namespace GenericConstraintsDemo.Utilities;

public static class EntityFactory
{
    public static TEntity CreateEntity<TEntity>(string id, Action<TEntity> configure)
        where TEntity : class, IEntity, new()
    {
        TEntity entity = new();
        entity.Id = id;
        configure(entity);

        return entity;
    }
}
