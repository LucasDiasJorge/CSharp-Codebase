using CsCheck;
using PropertyBasedTestingDemo.Money;
using Xunit.Abstractions;

namespace PropertyBasedTestingDemo.Tests;

/// <summary>
/// Dividir dinheiro em partes iguais: o requisito é uma propriedade, não uma lista de
/// exemplos.
/// </summary>
public sealed class MoneySplitterPropertyTests
{
    private readonly ITestOutputHelper _output;

    public MoneySplitterPropertyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(100.00, 4)]
    [InlineData(10.00, 2)]
    [InlineData(12.00, 3)]
    [InlineData(99.99, 3)]
    public void Exemplos_EscolhidosAMao_PassamAteNaVersaoIngenua(double total, int parts)
    {
        decimal amount = (decimal)total;

        IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenlyNaive(amount, parts);

        // Sao exatamente os exemplos que alguem escreveria: divisoes exatas.
        Assert.Equal(amount, shares.Sum());
    }

    [Fact]
    public void Propriedade_Conservacao_ReprovaAVersaoIngenua()
    {
        CsCheckException failure = PropertyFailure.Expect(() =>
            Generators.MoneyAndParts.Sample(
                input =>
                {
                    IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenlyNaive(input.Total, input.Parts);

                    Assert.Equal(input.Total, shares.Sum());
                },
                iter: 10_000));

        string counterexample = PropertyFailure.Counterexample(failure);

        _output.WriteLine("contraexemplo: " + counterexample);
        _output.WriteLine("encolhimentos: " + PropertyFailure.ShrinkCount(failure));

        // O contraexemplo e pequeno e legivel, mas NAO e sempre o mesmo: o defeito de
        // arredondamento ocorre para uma infinidade de pares. Ver ShrinkingTests.
        Assert.StartsWith("(", counterexample);
        Assert.True(PropertyFailure.ShrinkCount(failure) > 0, "o caso original foi encolhido");
    }

    [Fact]
    public void Propriedade_Conservacao_PassaNaVersaoCorreta() =>
        Generators.MoneyAndParts.Sample(
            input =>
            {
                IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenly(input.Total, input.Parts);

                Assert.Equal(input.Total, shares.Sum());
            },
            iter: 10_000);

    [Fact]
    public void Propriedade_Equidade_NenhumaParteDifereMaisDeUmCentavo() =>
        Generators.MoneyAndParts.Sample(
            input =>
            {
                IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenly(input.Total, input.Parts);

                Assert.True(shares.Max() - shares.Min() <= 0.01m);
            },
            iter: 10_000);

    [Fact]
    public void Propriedade_QuantidadeDePartes_ENaoNegatividade() =>
        Generators.MoneyAndParts.Sample(
            input =>
            {
                IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenly(input.Total, input.Parts);

                Assert.Equal(input.Parts, shares.Count);
                Assert.All(shares, share => Assert.True(share >= 0m));
            },
            iter: 10_000);

    /// <summary>
    /// A armadilha mais comum de teste baseado em propriedade: o gerador inventa
    /// entradas que o domínio não tem, e a propriedade reprova código correto.
    /// </summary>
    [Fact]
    public void Gerador_ForaDoDominio_ReprovaAImplementacaoCORRETA()
    {
        // Decimal arbitrario tem mais de duas casas — nao e dinheiro.
        Gen<(decimal Total, int Parts)> tooPrecise =
            Gen.Select(Gen.Decimal[0.001M, 1000M], Gen.Int[2, 5]);

        CsCheckException failure = PropertyFailure.Expect(() =>
            tooPrecise.Sample(
                input =>
                {
                    IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenly(input.Total, input.Parts);

                    Assert.Equal(input.Total, shares.Sum());
                },
                iter: 10_000));

        _output.WriteLine("contraexemplo: " + PropertyFailure.Counterexample(failure));
        _output.WriteLine(
            "a implementacao esta certa; o gerador que esta errado — dinheiro nao tem fracao de centavo.");

        // O mesmo efeito, a mao e sem aleatoriedade: uma entrada com fracao de centavo
        // nao pode ser dividida em centavos, e a soma nao fecha por definicao.
        IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenly(1.005m, 2);

        Assert.NotEqual(1.005m, shares.Sum());
        Assert.Equal(1.00m, shares.Sum());
    }
}
