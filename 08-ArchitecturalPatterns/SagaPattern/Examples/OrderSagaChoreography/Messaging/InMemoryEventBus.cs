using System.Collections.Concurrent;
using System.Linq;

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
        Type key = typeof(TEvent);

        List<Func<object, CancellationToken, Task>> list = _handlers.GetOrAdd(key, _ => new List<Func<object, CancellationToken, Task>>());

        lock (list)
        {
            list.Add((evt, ct) => handler((TEvent)evt, ct));
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));

        if (!_handlers.TryGetValue(typeof(TEvent), out List<Func<object, CancellationToken, Task>> list))
        {
            return;
        }

        List<Func<object, CancellationToken, Task>> snapshot;
        lock (list)
        {
            snapshot = list.ToList();
        }

        foreach (Func<object, CancellationToken, Task> handler in snapshot)
        {
            ct.ThrowIfCancellationRequested();
            await handler(@event, ct);
        }
    }
}
