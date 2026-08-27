using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Contrato do Advanced Resolver Pattern aplicado a pagamentos.
/// Duas propriedades sustentam o padrão em código de produção:
/// <see cref="AppliesTo"/>, que devolve a decisão de seleção para a própria estratégia, e
/// <see cref="Priority"/>, que torna a precedência um dado explícito do resolver em vez de
/// um efeito colateral da ordem de registro no container.
/// </summary>
public interface IPaymentResolver
{
    /// <summary>
    /// Ordem de avaliação: <b>menor valor é avaliado primeiro</b>, no mesmo espírito do
    /// <c>Order</c> de middlewares e filtros do ASP.NET Core.
    /// Resolvers mais específicos recebem prioridade menor que os genéricos que poderiam
    /// aceitar o mesmo pedido.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Responde se este resolver assume o pedido. Deve ser barato e livre de efeito colateral:
    /// é chamado para os candidatos em ordem de prioridade até o primeiro que aceitar.
    /// </summary>
    bool AppliesTo(PaymentRequest request);

    /// <summary>
    /// Liquida o lote de pedidos já filtrado para este resolver.
    /// </summary>
    Task ProcessAsync(IReadOnlyCollection<PaymentRequest> requests, CancellationToken cancellationToken);
}
