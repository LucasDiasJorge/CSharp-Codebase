using Microsoft.Extensions.Logging;
using Modules.Abstractions.Events;

namespace Host;

/// <summary>
/// Implementação do barramento para o monólito: entrega em processo, por chamada de
/// método. O ponto do modular monolith é que esta é a única peça que muda no dia em que
/// um módulo virar serviço — as assinaturas dos módulos continuam iguais.
/// </summary>
public sealed class InProcessEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Func<IntegrationEvent, CancellationToken, Task>>> _handlers = new();
    private readonly ILogger<InProcessEventBus> _logger;

    public InProcessEventBus(ILogger<InProcessEventBus> logger)
    {
        _logger = logger;
    }

    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : IntegrationEvent
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out List<Func<IntegrationEvent, CancellationToken, Task>>? list))
        {
            list = new List<Func<IntegrationEvent, CancellationToken, Task>>();
            _handlers[typeof(TEvent)] = list;
        }

        list.Add((integrationEvent, cancellationToken) => handler((TEvent)integrationEvent, cancellationToken));
    }

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IntegrationEvent
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out List<Func<IntegrationEvent, CancellationToken, Task>>? list))
        {
            return;
        }

        _logger.LogInformation("Bus: {Evento} publicado para {Quantidade} assinante(s).", typeof(TEvent).Name, list.Count);

        foreach (Func<IntegrationEvent, CancellationToken, Task> handler in list.ToArray())
        {
            try
            {
                await handler(integrationEvent, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Falha de um modulo nao pode derrubar os outros. Num monolito modular
                // isso e uma escolha; com broker de verdade, a entrega seria retentada.
                _logger.LogError("Bus: assinante de {Evento} falhou: {Erro}", typeof(TEvent).Name, ex.Message);
            }
        }
    }
}
