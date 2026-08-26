using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;
using StrategyResolver.Resolvers;

namespace StrategyResolver.Services;

/// <summary>
/// Orquestrador do Advanced Resolver Pattern. Este arquivo é o que nunca mais muda:
/// agrupa os pratos pelo discriminador, pergunta aos resolvers quem se aplica e dispara
/// os lotes em paralelo. Não há switch, nem <c>if</c> por tipo de cocção, nem referência
/// a uma implementação concreta além do fallback.
/// </summary>
public sealed class DishResolutionService : IDishResolutionService
{
    private readonly IReadOnlyList<IFoodCookingResolver> _resolvers;
    private readonly DefaultResolver _fallbackResolver;
    private readonly ILogger<DishResolutionService> _logger;

    public DishResolutionService(
        IEnumerable<IFoodCookingResolver> resolvers,
        DefaultResolver fallbackResolver,
        ILogger<DishResolutionService> logger)
    {
        ArgumentNullException.ThrowIfNull(resolvers);

        _resolvers = resolvers.ToList();
        _fallbackResolver = fallbackResolver ?? throw new ArgumentNullException(nameof(fallbackResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ResolveDishesAsync(IEnumerable<FoodDish> dishes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dishes);

        IEnumerable<IGrouping<string, FoodDish>> groups = dishes.GroupBy(dish => dish.CookingMethod);
        List<Task> cookingTasks = new List<Task>();

        foreach (IGrouping<string, FoodDish> group in groups)
        {
            IReadOnlyCollection<FoodDish> batch = group.ToList();
            IFoodCookingResolver resolver = SelectResolver(group.Key);

            _logger.LogInformation(
                "Método {CookingMethod} resolvido por {ResolverName} para {DishCount} prato(s).",
                group.Key,
                resolver.GetType().Name,
                batch.Count);

            cookingTasks.Add(resolver.CookAsync(batch, cancellationToken));
        }

        await Task.WhenAll(cookingTasks);
    }

    /// <summary>
    /// O coração do padrão. Percorre os candidatos na ordem de registro e fica com o primeiro
    /// que se declarar aplicável; se nenhum aceitar, cai no fallback explícito.
    /// Adicionar um método de cocção novo não toca neste método.
    /// </summary>
    private IFoodCookingResolver SelectResolver(string cookingMethod)
    {
        IFoodCookingResolver? resolver = _resolvers.FirstOrDefault(candidate => candidate.AppliesTo(cookingMethod));
        return resolver ?? _fallbackResolver;
    }
}
