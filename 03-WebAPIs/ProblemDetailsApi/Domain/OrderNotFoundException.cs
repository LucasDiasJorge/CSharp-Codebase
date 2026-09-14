namespace ProblemDetailsApi.Domain;

/// <summary>404: o recurso pedido não existe.</summary>
public sealed class OrderNotFoundException : DomainException
{
    public OrderNotFoundException(int orderId)
        : base(
            $"Pedido {orderId} nao encontrado.",
            StatusCodes.Status404NotFound,
            "https://example.com/erros/pedido-nao-encontrado",
            "Pedido nao encontrado")
    {
        OrderId = orderId;
    }

    public int OrderId { get; }

    public override IReadOnlyDictionary<string, object?> Extensions => new Dictionary<string, object?>
    {
        ["orderId"] = OrderId
    };
}
