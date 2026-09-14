namespace ProblemDetailsApi.Domain;

/// <summary>
/// 422: o corpo está sintaticamente correto e passou na validação de formato, mas
/// viola uma regra de negócio. É a distinção que separa 422 de 400 — o servidor
/// entendeu a requisição e mesmo assim não pode processá-la.
/// </summary>
public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(string sku, int requested, int available)
        : base(
            $"Estoque insuficiente para o SKU {sku}: pedidas {requested}, disponiveis {available}.",
            StatusCodes.Status422UnprocessableEntity,
            "https://example.com/erros/estoque-insuficiente",
            "Regra de negocio violada")
    {
        Sku = sku;
        Requested = requested;
        Available = available;
    }

    public string Sku { get; }

    public int Requested { get; }

    public int Available { get; }

    public override IReadOnlyDictionary<string, object?> Extensions => new Dictionary<string, object?>
    {
        ["sku"] = Sku,
        ["requested"] = Requested,
        ["available"] = Available
    };
}
