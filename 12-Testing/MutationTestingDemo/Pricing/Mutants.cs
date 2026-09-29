namespace MutationTestingDemo.Pricing;

/// <summary>
/// Um mutante: a mesma regra com <b>uma</b> alteração. É o que uma ferramenta de mutation
/// testing gera automaticamente no bytecode ou na árvore sintática; aqui eles são
/// escritos à mão para que o exemplo rode sem ferramenta externa e sempre igual.
/// </summary>
public sealed record Mutant(string Name, string Description, IPricingRules Rules);

/// <summary>
/// O catálogo de mutantes. Cada um corresponde a um operador de mutação clássico:
/// trocar limite de comparação, alterar constante, remover condicional, inverter
/// operador aritmético, substituir retorno por valor fixo.
/// </summary>
public static class Mutants
{
    public static IReadOnlyList<Mutant> All =>
    [
        new Mutant(
            "FronteiraAltaEstrita",
            ">= 500 virou > 500 (off-by-one na fronteira do desconto alto)",
            new MutatedRules(highTierStrict: true)),
        new Mutant(
            "TaxaAltaTrocada",
            "desconto alto de 10% virou 12%",
            new MutatedRules(highTierRate: 0.12m)),
        new Mutant(
            "FidelidadeIgnorada",
            "o bonus de 2% do cliente fidelidade nunca e aplicado",
            new MutatedRules(ignoreLoyalty: true)),
        new Mutant(
            "FreteFronteiraEstrita",
            ">= 300 virou > 300 na fronteira do frete gratis",
            new MutatedRules(freeShippingStrict: true)),
        new Mutant(
            "FreteSempreGratis",
            "o frete e sempre zero",
            new MutatedRules(shippingCost: 0m)),
        new Mutant(
            "SinalDoDescontoInvertido",
            "o total soma o desconto em vez de subtrair",
            new MutatedRules(addDiscountInsteadOfSubtracting: true)),
        new Mutant(
            "DescontoRemovido",
            "nenhum desconto e aplicado",
            new MutatedRules(noDiscount: true)),
    ];

    /// <summary>
    /// A regra com pontos de mutação parametrizados. Existe uma instância por mutante, e
    /// cada uma altera <b>uma só</b> coisa em relação a <see cref="PricingRules"/>.
    /// </summary>
    private sealed class MutatedRules : IPricingRules
    {
        private readonly bool _highTierStrict;
        private readonly decimal _highTierRate;
        private readonly bool _ignoreLoyalty;
        private readonly bool _freeShippingStrict;
        private readonly decimal _shippingCost;
        private readonly bool _addDiscountInsteadOfSubtracting;
        private readonly bool _noDiscount;

        public MutatedRules(
            bool highTierStrict = false,
            decimal highTierRate = PricingRules.HighTierRate,
            bool ignoreLoyalty = false,
            bool freeShippingStrict = false,
            decimal shippingCost = PricingRules.ShippingCost,
            bool addDiscountInsteadOfSubtracting = false,
            bool noDiscount = false)
        {
            _highTierStrict = highTierStrict;
            _highTierRate = highTierRate;
            _ignoreLoyalty = ignoreLoyalty;
            _freeShippingStrict = freeShippingStrict;
            _shippingCost = shippingCost;
            _addDiscountInsteadOfSubtracting = addDiscountInsteadOfSubtracting;
            _noDiscount = noDiscount;
        }

        public Charge Calculate(Cart cart)
        {
            bool isHighTier = _highTierStrict
                ? cart.Subtotal > PricingRules.HighTierThreshold
                : cart.Subtotal >= PricingRules.HighTierThreshold;

            decimal rate = isHighTier
                ? _highTierRate
                : cart.Subtotal >= PricingRules.LowTierThreshold
                    ? PricingRules.LowTierRate
                    : 0m;

            if (cart.IsLoyaltyMember && !_ignoreLoyalty)
            {
                rate += PricingRules.LoyaltyBonusRate;
            }

            decimal discount = _noDiscount ? 0m : Math.Round(cart.Subtotal * rate, 2);
            decimal afterDiscount = _addDiscountInsteadOfSubtracting
                ? cart.Subtotal + discount
                : cart.Subtotal - discount;

            bool freeShipping = _freeShippingStrict
                ? afterDiscount > PricingRules.FreeShippingThreshold
                : afterDiscount >= PricingRules.FreeShippingThreshold;

            decimal shipping = freeShipping ? 0m : _shippingCost;

            return new Charge(discount, shipping, afterDiscount + shipping);
        }
    }
}
