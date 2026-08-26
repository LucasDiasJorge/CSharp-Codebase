namespace StrategyResolver.Domain;

/// <summary>
/// Discriminadores usados pelos resolvers em <c>AppliesTo</c>.
/// São constantes de dados, não um enum: o valor costuma chegar de fora (banco, fila, payload),
/// e o exemplo precisa suportar um método desconhecido para exercitar o fallback.
/// </summary>
public static class CookingMethods
{
    public const string Oven = "OVEN";
    public const string Grill = "GRILL";
    public const string SousVide = "SOUS_VIDE";
}
