using System.Collections.Immutable;
using System.Reflection;
using Acme.EnumExtensions.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acme.EnumExtensions.Generator.Tests;

/// <summary>
/// Roda o gerador sobre um trecho de código, como o compilador rodaria.
///
/// Testar gerador é testar uma <b>função</b>: entra código, sai código e diagnósticos.
/// Não há projeto para compilar nem arquivo em disco.
/// </summary>
public sealed record GeneratorRun(
    GeneratorDriver Driver,
    GeneratorDriverRunResult Result,
    ImmutableArray<Diagnostic> Diagnostics,
    Compilation OutputCompilation)
{
    /// <summary>
    /// Erros de compilação do código <b>já com</b> o que o gerador produziu.
    ///
    /// Conferir na compilação ORIGINAL não funciona: o atributo injetado não existe lá, e
    /// o resultado é um <c>CS0246</c> enganoso.
    /// </summary>
    public IReadOnlyList<Diagnostic> OutputErrors =>
        OutputCompilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToList();

    /// <summary>Os arquivos gerados, sem o do atributo (que sai em toda execução).</summary>
    public IReadOnlyList<(string HintName, string Source)> GeneratedSources =>
        Result.Results
            .SelectMany(result => result.GeneratedSources)
            .Where(source => !source.HintName.Contains("Attribute"))
            .Select(source => (source.HintName, source.SourceText.ToString()))
            .ToList();

    public string SourceFor(string hintNamePart) =>
        GeneratedSources.Single(source => source.HintName.Contains(hintNamePart)).Source;
}

public static class GeneratorHarness
{
    private static readonly ImmutableArray<MetadataReference> References = BuildReferences();

    /// <summary>
    /// Executa o gerador uma vez. <c>trackIncrementalGeneratorSteps</c> ligado é o que
    /// permite depois inspecionar o cache — sem isso, os passos não são registrados.
    /// </summary>
    public static GeneratorRun Run(string source)
    {
        CSharpCompilation compilation = CreateCompilation(source);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new EnumExtensionsGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(
                disabledOutputs: IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true));

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out Compilation outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics);

        return new GeneratorRun(driver, driver.GetRunResult(), diagnostics, outputCompilation);
    }

    /// <summary>
    /// Executa, depois executa <b>de novo</b> sobre uma compilação equivalente. É assim
    /// que se verifica incrementalidade: o driver compara os modelos das duas execuções.
    /// </summary>
    public static (GeneratorRun First, GeneratorDriverRunResult Second) RunTwice(
        string firstSource,
        string? secondSource = null)
    {
        GeneratorRun first = Run(firstSource);

        CSharpCompilation second = CreateCompilation(secondSource ?? firstSource);

        GeneratorDriver driver = first.Driver.RunGeneratorsAndUpdateCompilation(
            second,
            out Compilation _,
            out ImmutableArray<Diagnostic> _);

        return (first, driver.GetRunResult());
    }

    public static CSharpCompilation CreateCompilation(string source) =>
        CSharpCompilation.Create(
            assemblyName: "Acme.Generated.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: References,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    /// <summary>
    /// Referências mínimas para o código de teste compilar. Sem elas, cada teste falharia
    /// com centenas de erros de tipo não encontrado, mascarando o que se quer verificar.
    /// </summary>
    private static ImmutableArray<MetadataReference> BuildReferences() =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Select(assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location))
            .ToImmutableArray();
}
