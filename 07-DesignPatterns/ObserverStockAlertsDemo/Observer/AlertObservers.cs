using Microsoft.Extensions.Logging;

namespace ObserverStockAlertsDemo.Observer;

/// <summary>Dispara alerta quando o preço passa de um teto.</summary>
public sealed class ThresholdAlertObserver : IStockObserver
{
    private readonly decimal _threshold;
    private readonly ILogger _logger;

    public ThresholdAlertObserver(string name, decimal threshold, ILogger logger)
    {
        Name = name;
        _threshold = threshold;
        _logger = logger;
    }

    public string Name { get; }

    public int AlertsFired { get; private set; }

    public void OnQuote(StockQuote quote)
    {
        if (quote.Price >= _threshold)
        {
            AlertsFired++;
            _logger.LogInformation("  [{Observador}] ALERTA: {Simbolo} chegou a {Preco:F2} (teto {Teto:F2}).", Name, quote.Symbol, quote.Price, _threshold);
        }
        else
        {
            _logger.LogInformation("  [{Observador}] {Simbolo} em {Preco:F2}, abaixo do teto.", Name, quote.Symbol, quote.Price);
        }
    }
}

/// <summary>Só registra o que passou, sem regra.</summary>
public sealed class AuditObserver : IStockObserver
{
    private readonly List<StockQuote> _seen = new List<StockQuote>();
    private readonly ILogger _logger;

    public AuditObserver(string name, ILogger logger)
    {
        Name = name;
        _logger = logger;
    }

    public string Name { get; }

    public int SeenCount => _seen.Count;

    public void OnQuote(StockQuote quote)
    {
        _seen.Add(quote);
        _logger.LogInformation("  [{Observador}] registrou {Simbolo} = {Preco:F2}.", Name, quote.Symbol, quote.Price);
    }
}

/// <summary>
/// Observador defeituoso. Existe para mostrar o que a falha de um assinante faz com os
/// outros — e que essa decisão é do subject, não do observador.
/// </summary>
public sealed class FaultyObserver : IStockObserver
{
    public FaultyObserver(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void OnQuote(StockQuote quote)
    {
        throw new InvalidOperationException($"{Name} nao conseguiu processar a cotacao");
    }
}

/// <summary>
/// Observador "pesado", usado para demonstrar o vazamento: se ele não se desinscrever,
/// a lista do subject continua segurando estes bytes para sempre.
/// </summary>
public sealed class HeavyObserver : IStockObserver
{
    private readonly byte[] _payload;

    public HeavyObserver(string name, int sizeInBytes)
    {
        Name = name;
        _payload = new byte[sizeInBytes];
    }

    public string Name { get; }

    public int PayloadSize => _payload.Length;

    public void OnQuote(StockQuote quote)
    {
        // Nao faz nada de util: o interesse aqui e o ciclo de vida, nao o comportamento.
    }
}
