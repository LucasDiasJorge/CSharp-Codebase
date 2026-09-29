using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Acme.EnumExtensions.Generator.Tests;

/// <summary>
/// A parte que dá nome ao <c>IIncrementalGenerator</c>: não refazer o que não mudou.
///
/// Isto não é detalhe de desempenho. Um gerador que invalida o cache a cada tecla digitada
/// roda em toda compilação incremental do editor, e o efeito é a IDE travando.
/// </summary>
public sealed class IncrementalityTests
{
    private const string BaseSource = """
        using Acme.EnumExtensions;

        namespace Loja;

        [EnumExtensions]
        public enum Status
        {
            Novo,
            Pago,
        }
        """;

    private readonly ITestOutputHelper _output;

    public IncrementalityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void EntradaIdentica_ReaproveitaOCache()
    {
        (GeneratorRun _, GeneratorDriverRunResult second) = GeneratorHarness.RunTwice(BaseSource);

        LogSteps(second, "entrada identica");

        Assert.All(
            OutputReasons(second),
            reason => Assert.Equal(IncrementalStepRunReason.Cached, reason));
    }

    /// <summary>
    /// Mudança em outro lugar do arquivo: a árvore muda, mas o modelo do enum não. O passo
    /// de saída continua em cache — é exatamente para isso que o modelo tem igualdade por
    /// valor.
    /// </summary>
    [Fact]
    public void MudancaQueNaoAfetaOEnum_MantemOCache()
    {
        (GeneratorRun _, GeneratorDriverRunResult second) = GeneratorHarness.RunTwice(
            BaseSource,
            BaseSource + """

                public class OutraCoisa
                {
                    public int Numero { get; set; }
                }
                """);

        LogSteps(second, "mudanca fora do enum");

        Assert.All(
            OutputReasons(second),
            reason => Assert.Equal(IncrementalStepRunReason.Cached, reason));
    }

    [Fact]
    public void MembroNovoNoEnum_InvalidaOCacheERegeraOArquivo()
    {
        const string withNewMember = """
            using Acme.EnumExtensions;

            namespace Loja;

            [EnumExtensions]
            public enum Status
            {
                Novo,
                Pago,
                Cancelado,
            }
            """;

        (GeneratorRun first, GeneratorDriverRunResult second) = GeneratorHarness.RunTwice(BaseSource, withNewMember);

        LogSteps(second, "membro novo no enum");

        // Agora o modelo mudou, e o passo de saida teve de rodar de novo.
        Assert.DoesNotContain(IncrementalStepRunReason.Cached, OutputReasons(second));

        // E o arquivo gerado reflete o membro novo.
        string regenerated = second.Results[0]
            .GeneratedSources
            .Single(source => source.HintName.Contains("Status"))
            .SourceText
            .ToString();

        Assert.DoesNotContain("Cancelado", first.SourceFor("Status"));
        Assert.Contains("global::Loja.Status.Cancelado => nameof(global::Loja.Status.Cancelado),", regenerated);
    }

    private void LogSteps(GeneratorDriverRunResult result, string label)
    {
        _output.WriteLine($"=== {label} ===");

        foreach (KeyValuePair<string, System.Collections.Immutable.ImmutableArray<IncrementalGeneratorRunStep>> step
            in result.Results[0].TrackedSteps)
        {
            string reasons = string.Join(
                ", ",
                step.Value.SelectMany(run => run.Outputs).Select(output => output.Reason));

            _output.WriteLine($"  {step.Key}: {reasons}");
        }
    }

    private static IReadOnlyList<IncrementalStepRunReason> OutputReasons(GeneratorDriverRunResult result) =>
        result.Results[0]
            .TrackedOutputSteps
            .SelectMany(step => step.Value)
            .SelectMany(step => step.Outputs)
            .Select(output => output.Reason)
            .ToList();
}
