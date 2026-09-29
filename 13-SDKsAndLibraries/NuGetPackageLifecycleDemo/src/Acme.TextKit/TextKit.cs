using System.Globalization;
using System.Text;

namespace Acme.TextKit;

/// <summary>
/// Utilitários de texto do pacote <c>Acme.TextKit</c>.
///
/// Cada membro público aqui tem documentação XML, e é ela que vai no pacote e alimenta
/// o IntelliSense de quem consome. Membro público sem <c>&lt;summary&gt;</c> gera aviso
/// <c>CS1591</c> quando <c>GenerateDocumentationFile</c> está ligado — o que é exatamente
/// o objetivo.
/// </summary>
public static class TextKit
{
    /// <summary>
    /// Converte um texto em slug: minúsculas, sem acentos, com hifens no lugar de
    /// espaços e pontuação.
    /// </summary>
    /// <param name="text">O texto de origem.</param>
    /// <returns>O slug correspondente, ou string vazia se não sobrar nada.</returns>
    /// <example>
    /// <code>
    /// TextKit.Slugify("Olá, Mundo Cruel!");  // "ola-mundo-cruel"
    /// </code>
    /// </example>
    public static string Slugify(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string withoutAccents = RemoveDiacritics(text).ToLowerInvariant();
        StringBuilder result = new StringBuilder(withoutAccents.Length);
        bool lastWasSeparator = false;

        foreach (char character in withoutAccents)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                result.Append(character);
                lastWasSeparator = false;

                continue;
            }

            // Sequencias de separadores colapsam em um unico hifen.
            if (!lastWasSeparator && result.Length > 0)
            {
                result.Append('-');
                lastWasSeparator = true;
            }
        }

        return result.ToString().Trim('-');
    }

    /// <summary>
    /// Encurta o texto para no máximo <paramref name="maxLength"/> caracteres,
    /// terminando em <c>"..."</c> quando houve corte.
    /// </summary>
    /// <param name="text">O texto de origem.</param>
    /// <param name="maxLength">Tamanho máximo do resultado, incluindo o sufixo.</param>
    /// <returns>O texto original, se já couber; caso contrário, o texto cortado.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Se <paramref name="maxLength"/> for menor que 4, que é o mínimo para caber um
    /// caractere mais o sufixo.
    /// </exception>
    public static string Truncate(string text, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 4);

        return text.Length <= maxLength
            ? text
            : string.Concat(text.AsSpan(0, maxLength - 3), "...");
    }

#if !BREAKING
    /// <summary>
    /// Conta as palavras do texto, tratando qualquer sequência de espaços como um único
    /// separador.
    /// </summary>
    /// <param name="text">O texto de origem.</param>
    /// <returns>A quantidade de palavras.</returns>
    /// <remarks>
    /// Este método está envolvido em <c>#if !BREAKING</c> de propósito: compilar com
    /// <c>-p:DefineConstants=BREAKING</c> o remove da API pública, e é assim que o
    /// exemplo demonstra o <c>PackageValidation</c> reprovando uma quebra de contrato.
    /// </remarks>
    public static int CountWords(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }
#endif

    private static string RemoveDiacritics(string text)
    {
        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder result = new StringBuilder(normalized.Length);

        foreach (char character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                result.Append(character);
            }
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
