using System.Collections.Concurrent;

namespace SagaPattern.Examples.OrderSagaChoreography.Messaging;

/// <summary>
/// Minimal in-memory event bus used only for demos.
/// Executes handlers sequentially in subscription order.
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, CancellationToken, Task>>> _handlers = new();

    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
    {
        var key = typeof(TEvent);

        var list = _handlers.GetOrAdd(key, _ => []);

        lock (list)
        {
            list.Add((evt, ct) => handler((TEvent)evt, ct));
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));

        if (!_handlers.TryGetValue(typeof(TEvent), out var list))
        {
            return;
        }

        List<Func<object, CancellationToken, Task>> snapshot;
        lock (list)
        {
            snapshot = [.. list];
        }

        foreach (var handler in snapshot)
        {
            ct.ThrowIfCancellationRequested();
            await handler(@event, ct);
        }
    }
}
