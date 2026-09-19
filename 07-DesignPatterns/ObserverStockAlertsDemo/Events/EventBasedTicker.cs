using Microsoft.Extensions.Logging;
using ObserverStockAlertsDemo.Observer;

namespace ObserverStockAlertsDemo.Events;

/// <summary>
/// O mesmo subject usando <c>event</c> do C#. O padrão Observer está embutido na
/// linguagem: <c>event</c> é uma lista de inscritos com sintaxe própria, e
/// <c>+=</c> / <c>-=</c> são o Attach e o Detach.
///
/// A diferença prática é o que se ganha e o que se perde. Ganha-se concisão e a
/// garantia de que ninguém de fora dispara o evento. Perde-se o controle sobre a
/// notificação: a invocação padrão para no primeiro assinante que lançar exceção.
/// </summary>
public sealed class EventBasedTicker
{
    private readonly ILogger<EventBasedTicker> _logger;

    public EventBasedTicker(ILogger<EventBasedTicker> logger)
    {
        _logger = logger;
    }

    /// <summary>Equivalente à lista de observadores, com açúcar sintático.</summary>
    public event EventHandler<StockQuote>? QuotePublished;

    public int SubscriberCount => QuotePublished?.GetInvocationList().Length ?? 0;

    /// <summary>Publicação simples: um throw em qualquer assinante interrompe o resto.</summary>
    public void Publish(StockQuote quote)
    {
        _logger.LogInformation("--- evento {Simbolo} = {Preco:F2} para {Total} assinante(s)", quote.Symbol, quote.Price, SubscriberCount);

        QuotePublished?.Invoke(this, quote);
    }

    /// <summary>
    /// Publicação com isolamento. Para conseguir isso é preciso percorrer a lista de
    /// invocação à mão — o operador <c>?.Invoke</c> não oferece essa opção.
    /// </summary>
    public void PublishIsolated(StockQuote quote)
    {
        _logger.LogInformation("--- evento isolado {Simbolo} = {Preco:F2}", quote.Symbol, quote.Price);

        EventHandler<StockQuote>? handlers = QuotePublished;
        if (handlers is null)
        {
            return;
        }

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try
            {
                ((EventHandler<StockQuote>)handler).Invoke(this, quote);
            }
            catch (Exception ex)
            {
                _logger.LogError("Assinante falhou: {Erro}. Os demais seguem.", ex.Message);
            }
        }
    }
}
