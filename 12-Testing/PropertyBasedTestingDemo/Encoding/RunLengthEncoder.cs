using System.Text;

namespace PropertyBasedTestingDemo.Encoding;

/// <summary>
/// Compressão por contagem de repetições (RLE), em duas versões.
///
/// A propriedade que define uma codificação é a <b>volta</b>:
/// <c>Decode(Encode(texto)) == texto</c>, para qualquer texto. Exemplo escolhido à mão
/// nunca inclui o caso que quebra isso, porque quem escreve o exemplo está pensando na
/// implementação que acabou de fazer.
/// </summary>
public static class RunLengthEncoder
{
    /// <summary>
    /// Versão ingênua: caractere seguido da contagem — <c>"aaab"</c> vira <c>"a3b1"</c>.
    /// Funciona perfeitamente até o texto conter um dígito, e aí a fronteira entre
    /// "dado" e "contagem" deixa de existir.
    /// </summary>
    public static string EncodeNaive(string input)
    {
        StringBuilder result = new StringBuilder();
        int index = 0;

        while (index < input.Length)
        {
            char current = input[index];
            int runLength = 1;

            while (index + runLength < input.Length && input[index + runLength] == current)
            {
                runLength++;
            }

            result.Append(current).Append(runLength);
            index += runLength;
        }

        return result.ToString();
    }

    /// <summary>
    /// A volta da versão ingênua: lê um caractere e depois consome os dígitos
    /// seguintes como contagem. Se o próprio dado era um dígito, ele é lido como parte
    /// do número.
    /// </summary>
    public static string DecodeNaive(string encoded)
    {
        StringBuilder result = new StringBuilder();
        int index = 0;

        while (index < encoded.Length)
        {
            char current = encoded[index];
            index++;

            int digitsStart = index;

            while (index < encoded.Length && char.IsAsciiDigit(encoded[index]))
            {
                index++;
            }

            // Sem digitos, assume uma ocorrencia — manter leniente para o teste falhar
            // pela propriedade, e nao por excecao de formato.
            int count = index > digitsStart
                ? int.Parse(encoded.AsSpan(digitsStart, index - digitsStart))
                : 1;

            result.Append(current, count);
        }

        return result.ToString();
    }

    /// <summary>
    /// Versão segura: contagem, <c>':'</c> e <b>exatamente um</b> caractere de dado.
    /// O <c>':'</c> encerra os dígitos, e o caractere seguinte é tomado como dado
    /// qualquer que seja — inclusive dígito ou o próprio <c>':'</c>.
    /// </summary>
    public static string Encode(string input)
    {
        StringBuilder result = new StringBuilder();
        int index = 0;

        while (index < input.Length)
        {
            char current = input[index];
            int runLength = 1;

            while (index + runLength < input.Length && input[index + runLength] == current)
            {
                runLength++;
            }

            result.Append(runLength).Append(':').Append(current);
            index += runLength;
        }

        return result.ToString();
    }

    /// <summary>
    /// A volta da versão segura.
    /// </summary>
    public static string Decode(string encoded)
    {
        StringBuilder result = new StringBuilder();
        int index = 0;

        while (index < encoded.Length)
        {
            int digitsStart = index;

            while (index < encoded.Length && char.IsAsciiDigit(encoded[index]))
            {
                index++;
            }

            if (index == digitsStart || index >= encoded.Length || encoded[index] != ':')
            {
                throw new FormatException($"esperava 'contagem:caractere' na posicao {digitsStart}");
            }

            int count = int.Parse(encoded.AsSpan(digitsStart, index - digitsStart));

            // Pula o ':' e toma UM caractere, sem interpretar.
            index++;

            if (index >= encoded.Length)
            {
                throw new FormatException("a codificacao termina sem o caractere de dado");
            }

            result.Append(encoded[index], count);
            index++;
        }

        return result.ToString();
    }
}
