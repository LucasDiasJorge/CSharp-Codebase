using System.Collections.Generic;
using GenericConstraintsDemo.Contracts;

namespace GenericConstraintsDemo.Infrastructure;

public sealed class EntityAuditSink<TEntity> : IEntitySink<TEntity>
    where TEntity : IEntity
{
    private readonly List<string> messages = new();

    public IReadOnlyList<string> Messages => messages;

    public void Write(TEntity entity)
    {
        messages.Add($"{typeof(TEntity).Name} recebeu entidade {entity.Id}.");
    }
}
