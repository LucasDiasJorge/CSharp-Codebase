namespace ProblemDetailsApi.Domain;

/// <summary>
/// 409: a requisição é válida, mas conflita com o estado atual do recurso. O cliente
/// não deve repeti-la sem antes reconsultar o recurso.
/// </summary>
public sealed class OrderAlreadyPaidException : DomainException
{
    public OrderAlreadyPaidException(int orderId, DateTimeOffset paidAt)
        : base(
            $"Pedido {orderId} ja foi pago e nao pode ser pago de novo.",
            StatusCodes.Status409Conflict,
            "https://example.com/erros/pedido-ja-pago",
            "Conflito com o estado do pedido")
    {
        OrderId = orderId;
        PaidAt = paidAt;
    }

    public int OrderId { get; }

    public DateTimeOffset PaidAt { get; }

    public override IReadOnlyDictionary<string, object?> Extensions => new Dictionary<string, object?>
    {
        ["orderId"] = OrderId,
        ["paidAt"] = PaidAt
    };
}
