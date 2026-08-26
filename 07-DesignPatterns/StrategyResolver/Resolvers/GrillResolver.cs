using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Resolver da grelha. Note que o preparo aqui é paralelo entre os pratos, enquanto o forno
/// é sequencial: cada estratégia mantém suas próprias decisões de execução sem vazar
/// nada para o orquestrador.
/// </summary>
public sealed class GrillResolver : IFoodCookingResolver
{
    private readonly ILogger<GrillResolver> _logger;

    public GrillResolver(ILogger<GrillResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool AppliesTo(string cookingMethod)
    {
        return string.Equals(cookingMethod, CookingMethods.Grill, StringComparison.OrdinalIgnoreCase);
    }

    public async Task CookAsync(IReadOnlyCollection<FoodDish> dishes, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Grelha acesa para {DishCount} prato(s).", dishes.Count);

        IEnumerable<Task> grillTasks = dishes.Select(dish => GrillOneAsync(dish, cancellationToken));
        await Task.WhenAll(grillTasks);
    }

    private async Task GrillOneAsync(FoodDish dish, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
        _logger.LogInformation("Grelhando {DishName} por {Minutes} min.", dish.Name, dish.Minutes);
    }
}
