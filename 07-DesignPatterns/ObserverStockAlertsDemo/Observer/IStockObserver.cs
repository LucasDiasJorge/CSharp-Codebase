namespace ObserverStockAlertsDemo.Observer;

/// <summary>
/// O contrato do observador na versão manual do padrão. É exatamente o que um
/// <c>event</c> do C# faz por baixo — a diferença é que aqui tudo está à vista.
/// </summary>
public interface IStockObserver
{
    string Name { get; }

    void OnQuote(StockQuote quote);
}
