namespace SagaPattern.Examples.OrderSagaChoreography.Messaging;

public interface IEventBus
{
    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler);
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default);
}
