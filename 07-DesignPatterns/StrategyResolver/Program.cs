using Microsoft.Extensions.DependencyInjection;
using StrategyResolver;
using StrategyResolver.Domain;
using StrategyResolver.Legacy;
using StrategyResolver.Services;

FoodDish[] pedido =
[
    new FoodDish("Lasanha", CookingMethods.Oven, 40),
    new FoodDish("Pão de alho", CookingMethods.Oven, 12),
    new FoodDish("Picanha", CookingMethods.Grill, 18),
    new FoodDish("Ovo mole", CookingMethods.SousVide, 45)
];

CancellationToken cancellationToken = CancellationToken.None;

// Etapa 1 - o ponto de partida: a decisão vive num switch dentro do serviço.
PrintHeader("1. Switch dentro do orquestrador (o que o padrão substitui)");
await using (ServiceProvider legacyProvider = CookingModule.BuildProvider(withSousVideResolver: false))
{
    SwitchCookingService switchService = legacyProvider.GetRequiredService<SwitchCookingService>();
    await switchService.CookAsync(pedido, cancellationToken);
}

// Etapa 2 - resolvers auto-descritivos, mas SOUS_VIDE ainda não tem quem o atenda.
PrintHeader("2. Resolvers registrados: OvenResolver e GrillResolver");
await using (ServiceProvider provider = CookingModule.BuildProvider(withSousVideResolver: false))
{
    IDishResolutionService resolutionService = provider.GetRequiredService<IDishResolutionService>();
    await resolutionService.ResolveDishesAsync(pedido, cancellationToken);
}

// Etapa 3 - o mesmo pedido e o mesmo DishResolutionService, com um resolver a mais registrado.
// Nenhuma linha do orquestrador mudou entre as etapas 2 e 3: só o composition root.
PrintHeader("3. SousVideResolver adicionado apenas no composition root");
await using (ServiceProvider provider = CookingModule.BuildProvider(withSousVideResolver: true))
{
    IDishResolutionService resolutionService = provider.GetRequiredService<IDishResolutionService>();
    await resolutionService.ResolveDishesAsync(pedido, cancellationToken);
}

static void PrintHeader(string title)
{
    Console.WriteLine();
    Console.WriteLine(new string('-', 72));
    Console.WriteLine(title);
    Console.WriteLine(new string('-', 72));
}
