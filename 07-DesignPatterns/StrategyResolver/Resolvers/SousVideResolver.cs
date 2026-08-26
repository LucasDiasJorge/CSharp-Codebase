using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Resolver adicionado depois que o sistema já estava em produção. É a peça que demonstra o
/// Open/Closed Principle no <c>Program</c>: passa a atender SOUS_VIDE apenas por ser registrado
/// no container, sem uma linha alterada em <c>DishResolutionService</c>.
/// </summary>
public sealed class SousVideResolver : IFoodCookingResolver
{
    private readonly ILogger<SousVideResolver> _logger;

    public SousVideResolver(ILogger<SousVideResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool AppliesTo(string cookingMethod)
    {
        return string.Equals(cookingMethod, CookingMethods.SousVide, StringComparison.OrdinalIgnoreCase);
    }

    public async Task CookAsync(IReadOnlyCollection<FoodDish> dishes, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Banho térmico estabilizado a 58 graus para {DishCount} prato(s).", dishes.Count);

        foreach (FoodDish dish in dishes)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
            _logger.LogInformation("Selando a vácuo e cozinhando {DishName} por {Minutes} min.", dish.Name, dish.Minutes);
        }
    }
}
