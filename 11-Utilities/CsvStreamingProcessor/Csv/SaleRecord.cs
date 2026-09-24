namespace CsvStreamingProcessor.Csv;

/// <summary>
/// Uma venda: o registro que o arquivo carrega.
/// </summary>
public sealed record SaleRecord(int Id, string Product, int Quantity, decimal UnitPrice, DateOnly Date, string Region)
{
    public decimal Total => Quantity * UnitPrice;
}

/// <summary>
/// Uma linha que não virou registro, com o número da linha e o motivo. Guardar o
/// número é o que torna o erro corrigível no arquivo de origem.
/// </summary>
public sealed record CsvError(int LineNumber, string RawLine, string Reason);

/// <summary>
/// Resultado de uma linha: ou um registro, ou um erro. Nunca os dois, nunca nenhum.
/// Modelar assim é o que permite continuar lendo em vez de lançar exceção e abortar.
/// </summary>
public sealed record CsvRow(SaleRecord? Record, CsvError? Error)
{
    public bool IsValid => Record is not null;

    public static CsvRow Ok(SaleRecord record) => new CsvRow(record, null);

    public static CsvRow Fail(int lineNumber, string rawLine, string reason) =>
        new CsvRow(null, new CsvError(lineNumber, rawLine, reason));
}
