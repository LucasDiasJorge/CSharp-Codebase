using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Contrato do Advanced Resolver Pattern. A diferença para o Strategy clássico está em
/// <see cref="AppliesTo"/>: a estratégia declara ela mesma quando é aplicável, em vez de o
/// orquestrador manter um switch mapeando dado -> implementação.
/// </summary>
public interface IFoodCookingResolver
{
    /// <summary>
    /// Responde se este resolver sabe tratar o discriminador recebido.
    /// Deve ser barato e livre de efeito colateral: é chamado para todos os candidatos
    /// até o primeiro que aceitar.
    /// </summary>
    bool AppliesTo(string cookingMethod);

    /// <summary>
    /// Executa o preparo do lote já filtrado para este resolver.
    /// </summary>
    Task CookAsync(IReadOnlyCollection<FoodDish> dishes, CancellationToken cancellationToken);
}
