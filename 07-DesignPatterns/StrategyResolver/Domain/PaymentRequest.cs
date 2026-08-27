namespace StrategyResolver.Domain;

/// <summary>
/// Ordem de pagamento a ser liquidada.
/// O objeto inteiro é passado para <c>AppliesTo</c>: o critério de seleção em um sistema real
/// raramente cabe em uma única string. Aqui, método e número de parcelas juntos decidem qual
/// resolver assume o pedido.
/// </summary>
public sealed record PaymentRequest(
    string OrderId,
    string Method,
    decimal Amount,
    int Installments);
