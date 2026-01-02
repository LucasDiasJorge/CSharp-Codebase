namespace CarriedEvent.Core;

/// <summary>
/// Classe base para eventos que carregam estado
/// </summary>
public abstract class CarriedStateEvent : IEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public abstract string EventType { get; }
    
    /// <summary>
    /// Versão do schema do evento para versionamento
    /// </summary>
    public int SchemaVersion { get; protected set; } = 1;

    protected CarriedStateEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }
}
