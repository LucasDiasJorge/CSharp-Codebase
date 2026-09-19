using Microsoft.Extensions.Logging;

namespace ObserverStockAlertsDemo.Observer;

/// <summary>
/// O subject escrito à mão. Mantém a lista de inscritos e avisa cada um a cada
/// cotação. Repare no que essa lista implica: o ticker guarda uma referência FORTE para
/// cada observador, e enquanto ele viver nenhum deles pode ser coletado.
/// </summary>
public sealed class StockTicker
{
    private readonly List<IStockObserver> _observers = new List<IStockObserver>();
    private readonly ILogger<StockTicker> _logger;
    private readonly bool _isolateFailures;

    public StockTicker(ILogger<StockTicker> logger, bool isolateFailures)
    {
        _logger = logger;
        _isolateFailures = isolateFailures;
    }

    public int ObserverCount => _observers.Count;

    public void Attach(IStockObserver observer)
    {
        _observers.Add(observer);
        _logger.LogInformation("{Observador} inscrito. Total: {Total}.", observer.Name, _observers.Count);
    }

    public bool Detach(IStockObserver observer)
    {
        bool removed = _observers.Remove(observer);

        _logger.LogInformation(
            "{Observador} {Resultado}. Total: {Total}.",
            observer.Name,
            removed ? "removido" : "NAO estava inscrito",
            _observers.Count);

        return removed;
    }

    public void Publish(StockQuote quote)
    {
        _logger.LogInformation("--- cotacao {Simbolo} = {Preco:F2} para {Total} observador(es)", quote.Symbol, quote.Price, _observers.Count);

        // Itera sobre uma copia: um observador que se desinscreve durante a notificacao
        // modificaria a lista em uso e derrubaria o laco com InvalidOperationException.
        foreach (IStockObserver observer in _observers.ToArray())
        {
            if (_isolateFailures)
            {
                try
                {
                    observer.OnQuote(quote);
                }
                catch (Exception ex)
                {
                    // Com isolamento, a falha de um observador nao impede os demais de
                    // receberem a notificacao.
                    _logger.LogError("Observador {Observador} falhou: {Erro}. Os demais seguem sendo notificados.", observer.Name, ex.Message);
                }
            }
            else
            {
                // Sem isolamento, a excecao sobe e interrompe o laco: quem estava depois
                // do observador defeituoso simplesmente nao e avisado.
                observer.OnQuote(quote);
            }
        }
    }
}
