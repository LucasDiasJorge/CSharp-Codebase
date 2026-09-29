using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Acme.EnumExtensions.Generator.Tests;

/// <summary>
/// O que o gerador produz: entradas, saídas e o fato de que a saída compila.
/// </summary>
public sealed class GenerationTests
{
    private const string MarkedEnum = """
        using Acme.EnumExtensions;

        namespace Loja;

        [EnumExtensions]
        public enum Status
        {
            Novo,
            Pago,
            Enviado,
        }
        """;

    private readonly ITestOutputHelper _output;

    public GenerationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void EnumMarcado_GeraUmArquivoComToStringFast()
    {
        GeneratorRun run = GeneratorHarness.Run(MarkedEnum);

        (string hintName, string source) = Assert.Single(run.GeneratedSources);

        _output.WriteLine(hintName);
        _output.WriteLine(source);

        Assert.Equal("Loja.Status.EnumExtensions.g.cs", hintName);
        Assert.Contains("public static class StatusExtensions", source);
        Assert.Contains("public static string ToStringFast(this global::Loja.Status value)", source);

        // Um braco de switch por membro, cada um devolvendo uma constante.
        Assert.Contains("global::Loja.Status.Novo => nameof(global::Loja.Status.Novo),", source);
        Assert.Contains("global::Loja.Status.Pago => nameof(global::Loja.Status.Pago),", source);
        Assert.Contains("global::Loja.Status.Enviado => nameof(global::Loja.Status.Enviado),", source);
    }

    /// <summary>
    /// Gerador que produz código que não compila é pior que gerador que não produz nada:
    /// o erro aparece no projeto de quem consome, apontando para um arquivo que a pessoa
    /// não escreveu.
    /// </summary>
    [Fact]
    public void CodigoGerado_CompilaSemErro()
    {
        GeneratorRun run = GeneratorHarness.Run(MarkedEnum);

        Assert.Empty(run.OutputErrors);
        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void EnumSemAtributo_NaoGeraNada()
    {
        GeneratorRun run = GeneratorHarness.Run("""
            namespace Loja;

            public enum SemAtributo
            {
                A,
                B,
            }
            """);

        // Nada alem do arquivo do atributo, que sai em toda execucao.
        Assert.Empty(run.GeneratedSources);
    }

    [Fact]
    public void EnumNoNamespaceGlobal_GeraSemDeclaracaoDeNamespace()
    {
        GeneratorRun run = GeneratorHarness.Run("""
            using Acme.EnumExtensions;

            [EnumExtensions]
            public enum Cor
            {
                Azul,
                Verde,
            }
            """);

        (string hintName, string source) = Assert.Single(run.GeneratedSources);

        Assert.Equal("global.Cor.EnumExtensions.g.cs", hintName);
        Assert.DoesNotContain("namespace", source);
        Assert.Contains("Cor.Azul => nameof(Cor.Azul),", source);
    }

    [Fact]
    public void EnumInterno_GeraExtensaoInterna()
    {
        GeneratorRun run = GeneratorHarness.Run("""
            using Acme.EnumExtensions;

            namespace Loja;

            [EnumExtensions]
            internal enum Interno
            {
                Um,
            }
            """);

        (string _, string source) = Assert.Single(run.GeneratedSources);

        Assert.Contains("internal static class InternoExtensions", source);
        Assert.DoesNotContain("public static class", source);
    }

    /// <summary>
    /// Diagnóstico em vez de código quebrado: um enum privado não pode receber extensão
    /// gerada em outro arquivo, e o gerador diz isso em vez de emitir algo que não compila.
    /// </summary>
    [Fact]
    public void EnumPrivadoAninhado_ReportaAEE001ENaoGeraArquivo()
    {
        GeneratorRun run = GeneratorHarness.Run("""
            using Acme.EnumExtensions;

            namespace Loja;

            public class Pedido
            {
                [EnumExtensions]
                private enum Escondido
                {
                    Um,
                }
            }
            """);

        Diagnostic diagnostic = Assert.Single(run.Diagnostics);

        _output.WriteLine(diagnostic.ToString());

        Assert.Equal("AEE001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("Escondido", diagnostic.GetMessage());
        Assert.Empty(run.GeneratedSources);
    }

    [Fact]
    public void VariosEnums_GeramUmArquivoCadaComNomeUnico()
    {
        GeneratorRun run = GeneratorHarness.Run("""
            using Acme.EnumExtensions;

            namespace Loja
            {
                [EnumExtensions]
                public enum Status { A }
            }

            namespace Entrega
            {
                [EnumExtensions]
                public enum Status { B }
            }
            """);

        // Mesmo NOME de enum em namespaces diferentes: os hint names precisam diferir,
        // senao o compilador reclama de fonte duplicada.
        Assert.Equal(2, run.GeneratedSources.Count);
        Assert.Contains(run.GeneratedSources, source => source.HintName == "Loja.Status.EnumExtensions.g.cs");
        Assert.Contains(run.GeneratedSources, source => source.HintName == "Entrega.Status.EnumExtensions.g.cs");
        Assert.Empty(run.OutputErrors);
    }

    [Fact]
    public void AtributoInjetado_SaiEmTodaExecucao()
    {
        GeneratorRun run = GeneratorHarness.Run("namespace Vazio;");

        GeneratedSourceResult attribute = Assert.Single(
            run.Result.Results[0].GeneratedSources,
            source => source.HintName == "EnumExtensionsAttribute.g.cs");

        Assert.Contains("internal sealed class EnumExtensionsAttribute", attribute.SourceText.ToString());
    }
}
