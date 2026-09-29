using WebApiIntegrationTestingDemo.Quotes;

namespace WebApiIntegrationTestingDemo.Tests;

/// <summary>
/// Substituto do provedor externo. Preço determinístico por símbolo — sem isso, todo
/// teste que compara total teria de aceitar qualquer número.
/// </summary>
public sealed class StubExchangeRateProvider : IExchangeRateProvider
{
    public const decimal DefaultPrice = 10.00m;

    private readonly Dictionary<string, decimal> _prices = new Dictionary<string, decimal>
    {
        ["PETR"] = 38.50m,
        ["VALE"] = 61.25m,
    };

    public int CallCount { get; private set; }

    public decimal GetUnitPrice(string symbol)
    {
        CallCount++;

        return _prices.TryGetValue(symbol, out decimal price) ? price : DefaultPrice;
    }
}
