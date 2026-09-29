using MutationTestingDemo.Pricing;
using Xunit.Abstractions;

namespace MutationTestingDemo.Tests;

/// <summary>
/// Mede o escore de mutação das duas suítes.
///
/// A conta é a da ferramenta: para cada mutante, roda a suíte; se alguma verificação
/// falha, o mutante foi <b>morto</b>; se todas passam, ele <b>sobreviveu</b> — e um
/// mutante vivo é uma mudança de comportamento que a suíte não percebe.
/// </summary>
public sealed class MutationScoreTests
{
    private readonly ITestOutputHelper _output;

    public MutationScoreTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SuiteFraca_TemCoberturaAltaEEscoreDeMutacaoBaixo()
    {
        MutationReport report = Run(WeakSuite.Checks, "suite fraca");

        // Cobertura nao e o mesmo que verificacao: esta suite executa todas as faixas e
        // percebe quase nada.
        Assert.True(report.Score < 50, $"esperava escore baixo, deu {report.Score}%");
        Assert.NotEmpty(report.Survivors);
    }

    [Fact]
    public void SuiteForte_MataTodosOsMutantes()
    {
        MutationReport report = Run(StrongSuite.Checks, "suite forte");

        Assert.Equal(100, report.Score);
        Assert.Empty(report.Survivors);
    }

    /// <summary>
    /// O mutante mais interessante do catálogo: trocar <c>&gt;=</c> por <c>&gt;</c> na
    /// fronteira. Só morre com um teste que use <b>exatamente</b> o valor do limite.
    /// </summary>
    [Fact]
    public void MutanteDeFronteira_SoMorreComTesteNoValorExatoDoLimite()
    {
        Mutant boundary = Mutants.All.Single(mutant => mutant.Name == "FronteiraAltaEstrita");

        // A suite fraca passa por 500, mas nao afirma o valor: o mutante sobrevive.
        Assert.False(Kills(WeakSuite.Checks, boundary), "a suite fraca nao deveria matar este mutante");

        // A suite forte tem um teste com subtotal exatamente 500.
        Assert.True(Kills(StrongSuite.Checks, boundary));

        // E o motivo: em 500, a regra correta da 10% e o mutante da 5%.
        Assert.Equal(50m, new PricingRules().Calculate(new Cart(1, 500m, false)).Discount);
        Assert.Equal(25m, boundary.Rules.Calculate(new Cart(1, 500m, false)).Discount);

        _output.WriteLine("subtotal 500 -> correto: 50,00 de desconto | mutante: 25,00");
    }

    /// <summary>
    /// As duas suítes rodando contra a implementação correta: as duas passam. É por isso
    /// que "todos os testes verdes" não diz nada sobre a força da suíte.
    /// </summary>
    [Fact]
    public void AsDuasSuites_PassamContraAImplementacaoCorreta()
    {
        PricingRules correct = new PricingRules();

        foreach (Check check in WeakSuite.Checks.Concat(StrongSuite.Checks))
        {
            check.Assert(correct);
        }

        _output.WriteLine(
            $"{WeakSuite.Checks.Count} verificacoes fracas + {StrongSuite.Checks.Count} fortes, todas verdes contra a regra correta.");
    }

    private MutationReport Run(IReadOnlyList<Check> checks, string label)
    {
        List<string> killed = new List<string>();
        List<string> survivors = new List<string>();

        foreach (Mutant mutant in Mutants.All)
        {
            if (Kills(checks, mutant))
            {
                killed.Add(mutant.Name);
            }
            else
            {
                survivors.Add($"{mutant.Name} — {mutant.Description}");
            }
        }

        int score = (int)Math.Round(100.0 * killed.Count / Mutants.All.Count);

        _output.WriteLine($"=== {label} ===");
        _output.WriteLine($"mortos: {killed.Count}/{Mutants.All.Count} (escore de mutacao {score}%)");

        foreach (string survivor in survivors)
        {
            _output.WriteLine("  SOBREVIVEU: " + survivor);
        }

        return new MutationReport(score, killed, survivors);
    }

    /// <summary>
    /// Um mutante morre quando <b>alguma</b> verificação da suíte falha contra ele.
    /// </summary>
    private static bool Kills(IReadOnlyList<Check> checks, Mutant mutant)
    {
        foreach (Check check in checks)
        {
            try
            {
                check.Assert(mutant.Rules);
            }
            catch (Exception)
            {
                return true;
            }
        }

        return false;
    }

    private sealed record MutationReport(int Score, IReadOnlyList<string> Killed, IReadOnlyList<string> Survivors);
}
