using GenericConstraintsDemo.Contracts;

namespace GenericConstraintsDemo.Utilities;

public static class ReferenceInspector
{
    public static string DescribeReference<TEntity>(TEntity entity)
        where TEntity : class, IEntity
    {
        return $"Referencia valida para {typeof(TEntity).Name} com Id {entity.Id}.";
    }
}
