namespace Modules.Abstractions.Events;

/// <summary>
/// Barramento em processo. Em um monólito modular ele é só uma chamada de método; o
/// ganho é que, no dia em que um módulo virar serviço, a assinatura não muda — troca-se
/// a implementação por uma que fala com um broker.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IntegrationEvent;

    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : IntegrationEvent;
}
