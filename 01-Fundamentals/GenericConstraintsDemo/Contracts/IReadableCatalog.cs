using System.Collections.Generic;

namespace GenericConstraintsDemo.Contracts;

public interface IReadableCatalog<out TEntity>
    where TEntity : class
{
    IReadOnlyCollection<TEntity> GetAll();
}
