using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Resolvers;

/// <summary>
/// Rede de segurança do padrão: garante que um discriminador desconhecido vire comportamento
/// observável e degradado, nunca uma <c>NullReferenceException</c> no orquestrador.
/// Fica fora da coleção de resolvers e é injetado à parte, para que o fallback não dependa
/// da ordem de registro (ver README).
/// </summary>
public sealed class DefaultResolver : IFoodCookingResolver
{
    private readonly ILogger<DefaultResolver> _logger;

    public DefaultResolver(ILogger<DefaultResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Aceita qualquer coisa: é o último recurso, escolhido explicitamente pelo serviço.
    /// </summary>
    public bool AppliesTo(string cookingMethod) => true;

    public Task CookAsync(IReadOnlyCollection<FoodDish> dishes, CancellationToken cancellationToken)
    {
        foreach (FoodDish dish in dishes)
        {
            _logger.LogWarning(
                "Nenhum resolver registrado para o método {CookingMethod}; {DishName} foi servido cru.",
                dish.CookingMethod,
                dish.Name);
        }

        return Task.CompletedTask;
    }
}
