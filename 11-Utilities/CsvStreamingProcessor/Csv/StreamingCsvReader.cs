using System.Globalization;

namespace CsvStreamingProcessor.Csv;

/// <summary>
/// Leitura por stream: uma linha por vez, sem nunca ter o arquivo inteiro em memória.
///
/// O <c>yield return</c> é o que sustenta a promessa — o método devolve o controle a
/// cada linha, e o <c>StreamReader</c> só avança quando o consumidor pede a próxima.
/// Trocar o retorno por <c>List&lt;CsvRow&gt;</c> materializa tudo e desfaz o exemplo.
/// </summary>
public static class StreamingCsvReader
{
    private const char Separator = ';';

    /// <summary>
    /// Percorre o arquivo devolvendo uma linha processada por vez. A cultura é
    /// parâmetro porque é ela que decide o que "129,90" significa.
    /// </summary>
    public static IEnumerable<CsvRow> ReadRows(string path, CultureInfo culture)
    {
        using StreamReader reader = new StreamReader(path);

        int lineNumber = 0;
        string? line = reader.ReadLine();

        // Cabecalho.
        lineNumber++;

        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            yield return ParseLine(line, lineNumber, culture);
        }
    }

    /// <summary>
    /// Converte uma linha em registro ou em erro. Nunca lança: uma linha ruim no meio
    /// de um arquivo de milhões não pode derrubar o processamento das outras.
    ///
    /// Público para que o cenário 1 possa comparar as duas estratégias de memória
    /// usando exatamente o mesmo parser — o que muda entre elas é só o que fica vivo.
    /// </summary>
    public static CsvRow ParseLine(string line, int lineNumber, CultureInfo culture)
    {
        IReadOnlyList<string> fields = CsvLineParser.Split(line, Separator);

        if (fields.Count != 6)
        {
            return CsvRow.Fail(lineNumber, line, $"esperados 6 campos, encontrados {fields.Count}");
        }

        if (!int.TryParse(fields[0], NumberStyles.Integer, culture, out int id))
        {
            return CsvRow.Fail(lineNumber, line, $"id invalido: '{fields[0]}'");
        }

        if (!int.TryParse(fields[2], NumberStyles.Integer, culture, out int quantity))
        {
            return CsvRow.Fail(lineNumber, line, $"quantidade invalida: '{fields[2]}'");
        }

        if (!decimal.TryParse(fields[3], NumberStyles.Number, culture, out decimal unitPrice))
        {
            return CsvRow.Fail(lineNumber, line, $"valor invalido para a cultura {culture.Name}: '{fields[3]}'");
        }

        if (!DateOnly.TryParse(fields[4], culture, DateTimeStyles.None, out DateOnly date))
        {
            return CsvRow.Fail(lineNumber, line, $"data invalida para a cultura {culture.Name}: '{fields[4]}'");
        }

        if (quantity <= 0)
        {
            return CsvRow.Fail(lineNumber, line, $"quantidade precisa ser positiva, veio {quantity}");
        }

        return CsvRow.Ok(new SaleRecord(id, fields[1], quantity, unitPrice, date, fields[5]));
    }
}
