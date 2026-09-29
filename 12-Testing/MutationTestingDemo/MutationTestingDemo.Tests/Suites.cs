using MutationTestingDemo.Pricing;

namespace MutationTestingDemo.Tests;

/// <summary>
/// Uma verificação nomeada sobre a regra de preço. Escrever as verificações como dados
/// é o que permite rodá-las <b>duas vezes</b>: contra a implementação correta (são
/// testes) e contra cada mutante (é a medição do escore de mutação).
/// </summary>
public sealed record Check(string Name, Action<IPricingRules> Assert);

/// <summary>
/// A suíte fraca. <b>Cobertura alta</b>: passa por todas as faixas de desconto, pelas
/// duas pernas do frete e pelo caminho do cliente fidelidade.
///
/// E <b>asserções sem conteúdo</b>: verifica que não lança, que o total é positivo, que
/// o desconto não é negativo. Cobertura de 100% e nenhuma afirmação sobre o valor certo.
/// </summary>
public static class WeakSuite
{
    private static readonly Cart[] Carts =
    [
        new Cart(1, 50m, false),
        new Cart(2, 199.99m, false),
        new Cart(3, 200m, false),
        new Cart(5, 499.99m, false),
        new Cart(6, 500m, false),
        new Cart(7, 500m, true),
        new Cart(9, 1_000m, true),
    ];

    public static IReadOnlyList<Check> Checks =>
    [
        new Check("nao lanca para nenhum carrinho", rules =>
        {
            foreach (Cart cart in Carts)
            {
                rules.Calculate(cart);
            }
        }),
        new Check("total e sempre positivo", rules =>
        {
            foreach (Cart cart in Carts)
            {
                Xunit.Assert.True(rules.Calculate(cart).Total > 0m);
            }
        }),
        new Check("desconto nunca e negativo", rules =>
        {
            foreach (Cart cart in Carts)
            {
                Xunit.Assert.True(rules.Calculate(cart).Discount >= 0m);
            }
        }),
        new Check("frete e zero ou o valor de tabela", rules =>
        {
            foreach (Cart cart in Carts)
            {
                decimal shipping = rules.Calculate(cart).Shipping;

                Xunit.Assert.True(shipping == 0m || shipping == PricingRules.ShippingCost);
            }
        }),
    ];
}

/// <summary>
/// A suíte forte. Mesma cobertura, asserções que fixam <b>valores exatos</b> e testam as
/// fronteiras dos dois lados.
/// </summary>
public static class StrongSuite
{
    public static IReadOnlyList<Check> Checks =>
    [
        new Check("abaixo da faixa baixa nao ha desconto", rules =>
        {
            Charge charge = rules.Calculate(new Cart(1, 199.99m, false));

            Xunit.Assert.Equal(0m, charge.Discount);
            Xunit.Assert.Equal(199.99m + PricingRules.ShippingCost, charge.Total);
        }),
        new Check("exatamente na faixa baixa o desconto e 5%", rules =>
        {
            Charge charge = rules.Calculate(new Cart(1, 200m, false));

            Xunit.Assert.Equal(10m, charge.Discount);
        }),
        new Check("um centavo abaixo da faixa alta ainda e 5%", rules =>
        {
            Charge charge = rules.Calculate(new Cart(1, 499.99m, false));

            Xunit.Assert.Equal(25m, charge.Discount);
        }),
        new Check("exatamente na faixa alta o desconto e 10%", rules =>
        {
            Charge charge = rules.Calculate(new Cart(1, 500m, false));

            Xunit.Assert.Equal(50m, charge.Discount);
        }),
        new Check("fidelidade soma exatamente 2 pontos percentuais", rules =>
        {
            Charge withoutLoyalty = rules.Calculate(new Cart(1, 500m, false));
            Charge withLoyalty = rules.Calculate(new Cart(1, 500m, true));

            Xunit.Assert.Equal(50m, withoutLoyalty.Discount);
            Xunit.Assert.Equal(60m, withLoyalty.Discount);
        }),
        new Check("frete cobrado quando o desconto derruba o valor abaixo do limite", rules =>
        {
            // 300 de subtotal com 5% de desconto vira 285: ABAIXO do limite de 300.
            Charge charge = rules.Calculate(new Cart(1, 300m, false));

            Xunit.Assert.Equal(15m, charge.Discount);
            Xunit.Assert.Equal(PricingRules.ShippingCost, charge.Shipping);

            // 320 com 5% -> 304, acima do limite: frete gratis.
            Charge free = rules.Calculate(new Cart(1, 320m, false));

            Xunit.Assert.Equal(16m, free.Discount);
            Xunit.Assert.Equal(0m, free.Shipping);
        }),

        // Esta verificacao foi acrescentada DEPOIS de um mutante sobreviver: nenhum teste
        // caia em exatamente 300 apos o desconto, porque o desconto move o valor. Achar a
        // entrada exigiu resolver S - round(S * 5%, 2) == 300, que da S = 315,79.
        new Check("frete gratis exatamente no limite, apos o desconto", rules =>
        {
            Charge charge = rules.Calculate(new Cart(1, 315.79m, false));

            Xunit.Assert.Equal(15.79m, charge.Discount);
            Xunit.Assert.Equal(0m, charge.Shipping);
            Xunit.Assert.Equal(300m, charge.Total);
        }),
        new Check("o total e subtotal menos desconto mais frete", rules =>
        {
            Charge charge = rules.Calculate(new Cart(4, 240m, true));

            // 240 * 7% = 16,80 -> 223,20 + 25 de frete.
            Xunit.Assert.Equal(16.80m, charge.Discount);
            Xunit.Assert.Equal(25m, charge.Shipping);
            Xunit.Assert.Equal(248.20m, charge.Total);
        }),
    ];
}
