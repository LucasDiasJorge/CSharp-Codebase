using System.Text;

namespace CsvStreamingProcessor.Csv;

/// <summary>
/// Divide uma linha de CSV respeitando aspas. Existe porque <c>line.Split(';')</c>
/// quebra no primeiro campo que contenha o separador — e campos de texto livre
/// contêm o separador com frequência.
/// </summary>
public static class CsvLineParser
{
    /// <summary>
    /// Divide a linha em campos. Um campo entre aspas pode conter o separador, e
    /// aspas duplas dentro dele representam uma aspa literal ("" -&gt; ").
    /// </summary>
    public static IReadOnlyList<string> Split(string line, char separator)
    {
        List<string> fields = new List<string>();
        StringBuilder current = new StringBuilder();
        bool insideQuotes = false;

        for (int index = 0; index < line.Length; index++)
        {
            char character = line[index];

            if (insideQuotes)
            {
                if (character != '"')
                {
                    current.Append(character);
                    continue;
                }

                // Aspa dentro de campo entre aspas: se vier outra em seguida, e uma
                // aspa literal; senao, o campo acabou.
                bool isEscapedQuote = index + 1 < line.Length && line[index + 1] == '"';

                if (isEscapedQuote)
                {
                    current.Append('"');
                    index++;
                }
                else
                {
                    insideQuotes = false;
                }

                continue;
            }

            if (character == '"' && current.Length == 0)
            {
                insideQuotes = true;
                continue;
            }

            if (character == separator)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        fields.Add(current.ToString());

        return fields;
    }
}
