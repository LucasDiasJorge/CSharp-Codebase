namespace StrategyResolver.Domain;

/// <summary>
/// Item de pedido a ser preparado. <see cref="CookingMethod"/> é o dado que carrega a decisão:
/// nenhum chamador precisa traduzi-lo para uma classe de estratégia — quem sabe interpretá-lo
/// é o próprio resolver, através de <c>AppliesTo</c>.
/// </summary>
public sealed record FoodDish(string Name, string CookingMethod, int Minutes);
