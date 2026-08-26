using StrategyResolver.Domain;

namespace StrategyResolver.Services;

/// <summary>
/// Porta de entrada do exemplo. O chamador entrega os pratos e não escolhe algoritmo algum:
/// a resolução acontece internamente, a partir do dado.
/// </summary>
public interface IDishResolutionService
{
    Task ResolveDishesAsync(IEnumerable<FoodDish> dishes, CancellationToken cancellationToken);
}
