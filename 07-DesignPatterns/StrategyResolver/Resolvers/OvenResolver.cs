using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Resolver do forno. Conhece apenas o próprio critério de aplicabilidade e o próprio preparo;
/// não conhece os demais resolvers nem o serviço que o seleciona.
/// </summary>
public sealed class OvenResolver : IFoodCookingResolver
{
    private readonly ILogger<OvenResolver> _logger;

    public OvenResolver(ILogger<OvenResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool AppliesTo(string cookingMethod)
    {
        return string.Equals(cookingMethod, CookingMethods.Oven, StringComparison.OrdinalIgnoreCase);
    }

    public async Task CookAsync(IReadOnlyCollection<FoodDish> dishes, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Forno pré-aquecido a 200 graus para {DishCount} prato(s).", dishes.Count);

        foreach (FoodDish dish in dishes)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
            _logger.LogInformation("Assando {DishName} por {Minutes} min.", dish.Name, dish.Minutes);
        }
    }
}
