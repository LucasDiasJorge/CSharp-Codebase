namespace MutationTestingDemo.Pricing;

public sealed record Cart(int ItemCount, decimal Subtotal, bool IsLoyaltyMember);

public sealed record Charge(decimal Discount, decimal Shipping, decimal Total);

public interface IPricingRules
{
    Charge Calculate(Cart cart);
}

/// <summary>
/// A regra correta. Tem exatamente o tipo de código que mutation testing existe para
/// avaliar: várias fronteiras numéricas, um bônus condicional e uma fórmula final.
///
/// Cada constante e cada operador aqui é um ponto onde um mutante pode nascer.
/// </summary>
public sealed class PricingRules : IPricingRules
{
    public const decimal HighTierThreshold = 500m;
    public const decimal LowTierThreshold = 200m;
    public const decimal HighTierRate = 0.10m;
    public const decimal LowTierRate = 0.05m;
    public const decimal LoyaltyBonusRate = 0.02m;
    public const decimal FreeShippingThreshold = 300m;
    public const decimal ShippingCost = 25m;

    public Charge Calculate(Cart cart)
    {
        decimal rate = cart.Subtotal >= HighTierThreshold
            ? HighTierRate
            : cart.Subtotal >= LowTierThreshold
                ? LowTierRate
                : 0m;

        if (cart.IsLoyaltyMember)
        {
            rate += LoyaltyBonusRate;
        }

        decimal discount = Math.Round(cart.Subtotal * rate, 2);
        decimal afterDiscount = cart.Subtotal - discount;
        decimal shipping = afterDiscount >= FreeShippingThreshold ? 0m : ShippingCost;

        return new Charge(discount, shipping, afterDiscount + shipping);
    }
}
