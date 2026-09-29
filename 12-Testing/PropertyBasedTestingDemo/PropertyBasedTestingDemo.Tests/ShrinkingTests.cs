using CsCheck;
using PropertyBasedTestingDemo.Money;
using Xunit.Abstractions;

namespace PropertyBasedTestingDemo.Tests;

/// <summary>
/// O shrinking é o que torna teste baseado em propriedade utilizável. Sem ele, a falha
/// vem com a entrada aleatória que a encontrou — um valor enorme e sem significado.
///
/// O que ele entrega é <b>um caso pequeno</b>, e não necessariamente <b>o menor caso</b>.
/// Os testes desta classe medem os dois comportamentos: quando o espaço de busca é
/// pequeno ele encosta na fronteira do defeito; quando é grande, ou quando o defeito tem
/// muitos mínimos locais, ele para em um caso pequeno qualquer.
/// </summary>
public sealed class ShrinkingTests
{
    private readonly ITestOutputHelper _output;

    public ShrinkingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Espaço de busca pequeno: o shrinking chega <b>na fronteira</b> do defeito. A
    /// propriedade só é falsa a partir de 900, e é 900 que ele reporta.
    /// </summary>
    [Fact]
    public void Shrinking_ChegaNaFronteiraQuandoOEspacoDeBuscaEPequeno()
    {
        for (int run = 1; run <= 3; run++)
        {
            CsCheckException failure = PropertyFailure.Expect(() =>
                Gen.Int[0, 1_000].Sample(value => Assert.True(value < 900), iter: 10_000));

            string counterexample = PropertyFailure.Counterexample(failure);

            _output.WriteLine(
                $"execucao {run}: contraexemplo {counterexample}, {PropertyFailure.ShrinkCount(failure)} encolhimentos");

            // Nunca abaixo de 900 (ali a propriedade e verdadeira), e praticamente em cima
            // da fronteira.
            Assert.InRange(int.Parse(counterexample), 900, 950);
        }
    }

    /// <summary>
    /// Mesmo defeito, espaço de busca mil vezes maior: o shrinking reduz muito, mas
    /// <b>não</b> chega na fronteira. Foram observados aqui 936, 971 e 1.048 — perto de
    /// 900, e nunca exatamente 900.
    ///
    /// A redução é de ordens de grandeza; a exatidão não é garantida.
    /// </summary>
    [Fact]
    public void Shrinking_NaoChegaNaFronteiraQuandoOEspacoDeBuscaEGrande()
    {
        for (int run = 1; run <= 3; run++)
        {
            CsCheckException failure = PropertyFailure.Expect(() =>
                Gen.Int[0, 1_000_000].Sample(value => Assert.True(value < 900), iter: 10_000));

            string counterexample = PropertyFailure.Counterexample(failure);

            _output.WriteLine(
                $"execucao {run}: contraexemplo {counterexample}, {PropertyFailure.ShrinkCount(failure)} encolhimentos");

            // Reducao de pelo menos uma ordem de grandeza sobre o espaco de 1.000.000,
            // sem promessa de exatidao.
            Assert.InRange(int.Parse(counterexample), 900, 100_000);
        }
    }

    /// <summary>
    /// Quando o defeito tem <b>muitos mínimos locais</b>, cada execução para em um caso
    /// pequeno diferente. Foram observados aqui, em execuções distintas da mesma
    /// propriedade: <c>(5.22, 8)</c>, <c>(8.79, 7)</c>, <c>(10.92, 10)</c> e
    /// <c>(52.85, 10)</c>.
    ///
    /// Não é defeito do shrinking: o bug de arredondamento ocorre para uma infinidade
    /// de pares, e não existe "o menor" par único.
    /// </summary>
    [Fact]
    public void Shrinking_NaoGaranteOMenorCasoQuandoHaVariosMinimos()
    {
        List<string> counterexamples = new List<string>();

        for (int run = 1; run <= 3; run++)
        {
            CsCheckException failure = PropertyFailure.Expect(() =>
                Generators.MoneyAndParts.Sample(
                    input =>
                    {
                        IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenlyNaive(input.Total, input.Parts);

                        Assert.Equal(input.Total, shares.Sum());
                    },
                    iter: 10_000));

            counterexamples.Add(PropertyFailure.Counterexample(failure));

            _output.WriteLine($"execucao {run}: contraexemplo {counterexamples[^1]}");
        }

        _output.WriteLine(
            counterexamples.Distinct().Count() == 1
                ? "as tres execucoes coincidiram desta vez — coincidencia, nao garantia."
                : "contraexemplos diferentes: nao ha um minimo unico para este defeito.");

        // O que o shrinking GARANTE: o caso reportado continua sendo uma entrada que o
        // gerador poderia produzir. Ele encolhe dentro do dominio, nunca fora dele.
        Assert.All(counterexamples, counterexample =>
        {
            (decimal total, int parts) = ParseCounterexample(counterexample);

            Assert.InRange(total, 0.01m, 100_000m);
            Assert.InRange(parts, 1, 12);
            Assert.Equal(total, Math.Round(total, 2));
        });
    }

    /// <summary>
    /// O contraexemplo mínimo absoluto deste bug, verificado à mão: <c>0,01</c> em duas
    /// partes. Nenhuma execução precisa achá-lo para o bug ficar entendido — mas ele é
    /// o enunciado mais curto possível do defeito.
    /// </summary>
    [Fact]
    public void OMenorCasoAbsoluto_EOEnunciadoDoBug()
    {
        IReadOnlyList<decimal> shares = MoneySplitter.SplitEvenlyNaive(0.01m, 2);

        _output.WriteLine($"0,01 em 2 partes (ingenua): [{string.Join(", ", shares)}] soma {shares.Sum()}");

        // Arredonda 0,005 para 0,00 duas vezes: o centavo desaparece.
        Assert.Equal(0.00m, shares.Sum());

        IReadOnlyList<decimal> correct = MoneySplitter.SplitEvenly(0.01m, 2);

        _output.WriteLine($"0,01 em 2 partes (correta): [{string.Join(", ", correct)}] soma {correct.Sum()}");

        Assert.Equal(0.01m, correct.Sum());
        Assert.Equal(new[] { 0.01m, 0.00m }, correct);
    }

    /// <summary>
    /// Lê o par <c>(total, partes)</c> que o CsCheck imprime.
    /// </summary>
    private static (decimal Total, int Parts) ParseCounterexample(string counterexample)
    {
        string[] parts = counterexample.Trim('(', ')').Split(',');

        return (
            decimal.Parse(parts[0].Trim().TrimEnd('M', 'm'), System.Globalization.CultureInfo.InvariantCulture),
            int.Parse(parts[1].Trim()));
    }
}
