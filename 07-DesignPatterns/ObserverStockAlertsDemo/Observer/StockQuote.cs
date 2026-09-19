namespace ObserverStockAlertsDemo.Observer;

/// <summary>Cotação publicada pelo subject.</summary>
public sealed record StockQuote(string Symbol, decimal Price, DateTimeOffset At);
