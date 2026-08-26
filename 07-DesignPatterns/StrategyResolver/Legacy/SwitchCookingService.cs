using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Legacy;

/// <summary>
/// Ponto de partida descrito no artigo: a decisão de qual algoritmo usar mora em um switch,
/// dentro do serviço que orquestra. Três problemas ficam visíveis aqui e desaparecem em
/// <c>DishResolutionService</c>:
/// 1. cada método de cocção novo obriga a editar este arquivo (viola Open/Closed);
/// 2. o serviço precisa conhecer todas as implementações concretas;
/// 3. o preparo de cada método fica misturado no mesmo corpo, sem fronteira de teste.
/// Mantido apenas como contraste didático — não é o caminho recomendado.
/// </summary>
public sealed class SwitchCookingService
{
    private readonly ILogger<SwitchCookingService> _logger;

    public SwitchCookingService(ILogger<SwitchCookingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task CookAsync(IEnumerable<FoodDish> dishes, CancellationToken cancellationToken)
    {
        foreach (FoodDish dish in dishes)
        {
            switch (dish.CookingMethod)
            {
                case CookingMethods.Oven:
                    _logger.LogInformation("[switch] Assando {DishName} no forno.", dish.Name);
                    break;

                case CookingMethods.Grill:
                    _logger.LogInformation("[switch] Grelhando {DishName}.", dish.Name);
                    break;

                // Todo método novo entra como mais um case aqui dentro.
                default:
                    _logger.LogWarning("[switch] Método {CookingMethod} não previsto no switch.", dish.CookingMethod);
                    break;
            }
        }

        return Task.CompletedTask;
    }
}
