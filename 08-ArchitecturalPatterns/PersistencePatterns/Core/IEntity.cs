namespace PersistencePatterns.Core;

/// <summary>
/// Interface base para entidades com identificador
/// </summary>
public interface IEntity<TId>
{
    TId Id { get; }
}
