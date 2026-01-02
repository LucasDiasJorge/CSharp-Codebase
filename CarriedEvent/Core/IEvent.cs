namespace CarriedEvent.Core;

/// <summary>
/// Interface base para eventos
/// </summary>
public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}
