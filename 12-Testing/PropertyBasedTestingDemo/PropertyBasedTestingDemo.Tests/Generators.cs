using CsCheck;

namespace PropertyBasedTestingDemo.Tests;

/// <summary>
/// Os geradores usados pelas propriedades. Ficam juntos porque a escolha do gerador é
/// parte do teste: ele define o que conta como entrada válida do domínio.
/// </summary>
public static class Generators
{
    /// <summary>
    /// Dinheiro e número de partes. O total é gerado em <b>centavos</b> e depois
    /// dividido por 100 — dinheiro tem duas casas decimais, e gerar mais do que isso
    /// testaria um requisito que não existe.
    /// </summary>
    public static Gen<(decimal Total, int Parts)> MoneyAndParts =>
        Gen.Select(Gen.Int[1, 10_000_000].Select(cents => cents / 100m), Gen.Int[1, 12]);

    /// <summary>
    /// Texto curto sobre um alfabeto pequeno que <b>inclui dígitos</b> e <c>':'</c>.
    /// É o que faz o gerador achar o caso interessante em milhares de tentativas em
    /// vez de bilhões.
    /// </summary>
    public static Gen<string> Text =>
        Gen.Char['0', 'c'].Array[0, 8].Select(characters => new string(characters));
}
