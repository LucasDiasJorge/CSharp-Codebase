namespace GenericConstraintsDemo.Contracts;

public interface IEntitySink<in TEntity>
    where TEntity : IEntity
{
    void Write(TEntity entity);
}
