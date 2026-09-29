using CsCheck;
using PropertyBasedTestingDemo.Encoding;
using Xunit.Abstractions;

namespace PropertyBasedTestingDemo.Tests;

/// <summary>
/// Codificação e decodificação: a propriedade da volta (<i>round-trip</i>) é a mais
/// barata de escrever e uma das que mais acham bug.
/// </summary>
public sealed class RunLengthPropertyTests
{
    private readonly ITestOutputHelper _output;

    public RunLengthPropertyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("aaabbc")]
    [InlineData("aaaaaaaaaa")]
    public void Exemplos_ClassicosPassamAteNaVersaoIngenua(string input)
    {
        // Os exemplos de todo tutorial de RLE. Nenhum deles tem digito no dado.
        Assert.Equal(input, RunLengthEncoder.DecodeNaive(RunLengthEncoder.EncodeNaive(input)));
    }

    [Fact]
    public void RoundTrip_ReprovaAVersaoIngenua()
    {
        CsCheckException failure = PropertyFailure.Expect(() =>
            Generators.Text.Sample(
                text => Assert.Equal(text, RunLengthEncoder.DecodeNaive(RunLengthEncoder.EncodeNaive(text))),
                iter: 10_000));

        string counterexample = PropertyFailure.Counterexample(failure);

        _output.WriteLine("contraexemplo minimo: " + counterexample);
        _output.WriteLine("encolhimentos: " + PropertyFailure.ShrinkCount(failure));

        // Dois caracteres bastam, e um deles e digito: e o caso que nenhum exemplo
        // escrito a mao inclui.
        string minimal = counterexample.Trim('"');

        Assert.Equal(2, minimal.Length);
        Assert.Contains(minimal.ToCharArray(), character => char.IsAsciiDigit(character));
    }

    [Fact]
    public void RoundTrip_PassaNaVersaoSegura() =>
        Generators.Text.Sample(
            text => Assert.Equal(text, RunLengthEncoder.Decode(RunLengthEncoder.Encode(text))),
            iter: 10_000);

    /// <summary>
    /// Propriedade metamórfica: não afirma o resultado, afirma uma <b>relação</b> entre
    /// dois resultados. Serve quando não se sabe calcular a resposta esperada.
    /// </summary>
    [Fact]
    public void Metamorfica_ConcatenarNuncaAumentaOTamanhoCodificado() =>
        Gen.Select(Generators.Text, Generators.Text).Sample(
            pair =>
            {
                int joined = RunLengthEncoder.Encode(pair.Item1 + pair.Item2).Length;
                int separate = RunLengthEncoder.Encode(pair.Item1).Length + RunLengthEncoder.Encode(pair.Item2).Length;

                // Concatenar pode fundir duas corridas; nunca pode piorar.
                Assert.True(joined <= separate, $"'{pair.Item1}' + '{pair.Item2}': {joined} > {separate}");
            },
            iter: 10_000);

    /// <summary>
    /// Teste diferencial: usa uma implementação como oráculo da outra. Aqui a versão
    /// segura é a referência, e a propriedade aponta onde a ingênua discorda dela.
    /// </summary>
    [Fact]
    public void Diferencial_AVersaoSeguraServeDeOraculoParaAIngenua()
    {
        CsCheckException failure = PropertyFailure.Expect(() =>
            Generators.Text.Sample(
                text =>
                {
                    string viaNaive = RunLengthEncoder.DecodeNaive(RunLengthEncoder.EncodeNaive(text));
                    string viaSafe = RunLengthEncoder.Decode(RunLengthEncoder.Encode(text));

                    Assert.Equal(viaSafe, viaNaive);
                },
                iter: 10_000));

        _output.WriteLine("as duas implementacoes discordam em: " + PropertyFailure.Counterexample(failure));
    }
}
