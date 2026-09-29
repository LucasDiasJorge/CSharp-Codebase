using System.ComponentModel.DataAnnotations;

namespace WebApiIntegrationTestingDemo.Quotes;

public enum QuoteStatus
{
    Open,
    Expired,
}

/// <summary>
/// O que a API recebe. As anotações produzem o 400 com <c>ProblemDetails</c> — parte do
/// contrato HTTP que só um teste de integração observa.
/// </summary>
public sealed class CreateQuoteRequest
{
    [Required]
    [RegularExpression("^[A-Z]{3,5}$", ErrorMessage = "symbol deve ter de 3 a 5 letras maiusculas")]
    public string Symbol { get; set; } = string.Empty;

    [Range(1, 1_000, ErrorMessage = "quantity deve estar entre 1 e 1000")]
    public int Quantity { get; set; }
}

/// <summary>
/// O que a API devolve. O nome das propriedades aqui é PascalCase; o JSON sai em
/// camelCase, e essa diferença é contrato — não detalhe.
/// </summary>
public sealed record QuoteResponse(
    Guid Id,
    string Symbol,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    QuoteStatus Status,
    DateTimeOffset CreatedAt);

/// <summary>
/// Preço unitário de um símbolo. Em produção isto é uma chamada HTTP a um provedor
/// externo — é a dependência que o teste de integração precisa substituir.
/// </summary>
public interface IExchangeRateProvider
{
    decimal GetUnitPrice(string symbol);
}

/// <summary>
/// Implementação "de produção": depende de rede. Falha alto e claro se alguém a
/// executar em teste, em vez de travar por timeout.
/// </summary>
public sealed class RemoteExchangeRateProvider : IExchangeRateProvider
{
    public decimal GetUnitPrice(string symbol) =>
        throw new ExchangeRateUnavailableException(
            $"o provedor remoto precisa de rede para cotar '{symbol}'");
}

public sealed class ExchangeRateUnavailableException : Exception
{
    public ExchangeRateUnavailableException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Repositório em memória. Cada instância da aplicação tem o seu — o que faz o
/// isolamento entre testes depender de quantas instâncias existem.
/// </summary>
public sealed class QuoteRepository
{
    private readonly Dictionary<Guid, QuoteResponse> _quotes = new Dictionary<Guid, QuoteResponse>();

    public int Count => _quotes.Count;

    public void Add(QuoteResponse quote) => _quotes[quote.Id] = quote;

    public QuoteResponse? Find(Guid id) => _quotes.TryGetValue(id, out QuoteResponse? quote) ? quote : null;
}
