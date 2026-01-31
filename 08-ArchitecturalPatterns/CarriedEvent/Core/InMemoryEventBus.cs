using System.Collections.Concurrent;

namespace CarriedEvent.Core;

/// <summary>
/// Implementação em memória do barramento de eventos
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        
        _handlers.AddOrUpdate(
            eventType,
            _ => [handler],
            (_, list) => { list.Add(handler); return list; }
        );

        Console.WriteLine($"  [EventBus] Handler registrado para {eventType.Name}");
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        
        Console.WriteLine($"\n  [EventBus] Publicando evento: {@event.EventType}");
        Console.WriteLine($"  [EventBus] EventId: {@event.EventId}");

        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers.Cast<IEventHandler<TEvent>>())
            {
                await handler.HandleAsync(@event, ct);
            }
        }
        else
        {
            Console.WriteLine($"  [EventBus] Nenhum handler registrado para {eventType.Name}");
        }
    }
}
