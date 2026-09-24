using System.Diagnostics;
using System.Globalization;
using System.Text;
using CsvStreamingProcessor.Csv;
using Microsoft.Extensions.Logging;

namespace CsvStreamingProcessor.Demo;

/// <summary>
/// Cinco cenários: carregar tudo contra streaming, cultura que corrompe em silêncio,
/// linhas inválidas que não podem derrubar o lote, aspas com separador dentro e o
/// pipeline que lê, transforma e escreve sem materializar nada.
/// </summary>
public sealed class CsvDemoRunner
{
    private const int LargeFileRows = 200_000;

    private readonly ILogger<CsvDemoRunner> _logger;

    public CsvDemoRunner(ILogger<CsvDemoRunner> logger)
    {
        _logger = logger;
    }

    public void RunAll()
    {
        string largeFile = SampleFileFactory.CreateLargeFile(LargeFileRows);
        string messyFile = SampleFileFactory.CreateMessyFile();

        RunMemoryComparison(largeFile);
        RunCultureTrap();
        RunInvalidRecords(messyFile);
        RunQuotedFields();
        RunIncrementalPipeline(largeFile);

        SampleFileFactory.Cleanup();
    }

    private void RunMemoryComparison(string path)
    {
        Section("1. Carregar tudo contra ler por stream");

        long fileBytes = new FileInfo(path).Length;

        _logger.LogInformation(
            "  arquivo: {Linhas:N0} linhas, {Tamanho:N1} MB em disco",
            LargeFileRows,
            fileBytes / 1024.0 / 1024.0);

        CultureInfo brazil = CultureInfo.GetCultureInfo("pt-BR");

        // As duas estrategias usam o MESMO parser e produzem o mesmo resultado. O que
        // muda e o que continua vivo no fim: uma retem todos os registros, a outra
        // nenhum. Por isso o streaming e medido primeiro, com a memoria limpa.
        Collect();
        long beforeStream = GC.GetTotalMemory(forceFullCollection: true);
        Stopwatch watch = Stopwatch.StartNew();
        int streamedCount = 0;

        foreach (CsvRow row in StreamingCsvReader.ReadRows(path, brazil))
        {
            if (row.IsValid)
            {
                streamedCount++;
            }
        }

        long afterStream = GC.GetTotalMemory(forceFullCollection: true);
        long streamMs = watch.ElapsedMilliseconds;

        Collect();
        long beforeAll = GC.GetTotalMemory(forceFullCollection: true);
        watch.Restart();

        string[] allLines = File.ReadAllLines(path);
        List<SaleRecord> allRecords = new List<SaleRecord>(allLines.Length);

        for (int index = 1; index < allLines.Length; index++)
        {
            CsvRow row = StreamingCsvReader.ParseLine(allLines[index], index + 1, brazil);

            if (row.Record is SaleRecord record)
            {
                allRecords.Add(record);
            }
        }

        long afterAll = GC.GetTotalMemory(forceFullCollection: true);
        long readAllMs = watch.ElapsedMilliseconds;
        int allCount = allRecords.Count;

        GC.KeepAlive(allLines);
        GC.KeepAlive(allRecords);

        _logger.LogInformation(
            "  ReadAllLines + List: {Linhas:N0} registros, {Memoria} MB retidos, {Tempo}ms",
            allCount,
            FormatMegabytes(afterAll - beforeAll),
            readAllMs);

        _logger.LogInformation(
            "  streaming (yield):   {Linhas:N0} registros, {Memoria} MB retidos, {Tempo}ms",
            streamedCount,
            FormatMegabytes(afterStream - beforeStream),
            streamMs);

        _logger.LogInformation(
            "O streaming aloca as mesmas strings, mas nao as SEGURA: cada linha vira lixo assim que o consumidor termina com ela. A memoria retida nao cresce com o arquivo.");
    }

    private void RunCultureTrap()
    {
        Section("2. Cultura: o mesmo texto, dois numeros diferentes, sem erro");

        CultureInfo brazil = CultureInfo.GetCultureInfo("pt-BR");
        CultureInfo invariant = CultureInfo.InvariantCulture;

        foreach (string text in new[] { "129,90", "1.234", "1.234,56", "1,234.56" })
        {
            bool brOk = decimal.TryParse(text, NumberStyles.Number, brazil, out decimal brValue);
            bool invOk = decimal.TryParse(text, NumberStyles.Number, invariant, out decimal invValue);

            _logger.LogInformation(
                "  \"{Texto,-9}\" -> pt-BR: {Br,-12} | invariant: {Inv,-12} {Alerta}",
                text,
                brOk ? brValue.ToString(invariant) : "erro",
                invOk ? invValue.ToString(invariant) : "erro",
                brOk && invOk && brValue != invValue ? "<- os dois aceitaram, com valores DIFERENTES" : string.Empty);
        }

        foreach (string text in new[] { "2026-01-15", "15/01/2026", "01/02/2026" })
        {
            bool brOk = DateOnly.TryParse(text, brazil, DateTimeStyles.None, out DateOnly brDate);
            bool invOk = DateOnly.TryParse(text, invariant, DateTimeStyles.None, out DateOnly invDate);

            _logger.LogInformation(
                "  \"{Texto,-10}\" -> pt-BR: {Br,-10} | invariant: {Inv,-10} {Alerta}",
                text,
                brOk ? brDate.ToString("yyyy-MM-dd") : "erro",
                invOk ? invDate.ToString("yyyy-MM-dd") : "erro",
                brOk && invOk && brDate != invDate ? "<- mesmo dia? nao: dia e mes trocados" : string.Empty);
        }

        _logger.LogWarning(
            "O perigo nao e a excecao — e o caso em que as duas culturas ACEITAM e discordam. Nenhum log, nenhum erro, numero errado gravado.");
    }

    private void RunInvalidRecords(string path)
    {
        Section("3. Linhas invalidas nao podem derrubar o lote");

        List<SaleRecord> valid = new List<SaleRecord>();
        List<CsvError> errors = new List<CsvError>();

        foreach (CsvRow row in StreamingCsvReader.ReadRows(path, CultureInfo.GetCultureInfo("pt-BR")))
        {
            if (row.Record is SaleRecord record)
            {
                valid.Add(record);
            }
            else if (row.Error is CsvError error)
            {
                errors.Add(error);
            }
        }

        _logger.LogInformation("  {Validos} registros validos, {Invalidos} rejeitados:", valid.Count, errors.Count);

        foreach (CsvError error in errors)
        {
            _logger.LogWarning("    linha {Linha}: {Motivo}", error.LineNumber, error.Reason);
        }

        _logger.LogInformation(
            "O numero da linha e o que torna o erro corrigivel. \"falha ao importar\" sem a linha obriga quem recebe a procurar em milhoes de registros.");
    }

    private void RunQuotedFields()
    {
        Section("4. Aspas: onde o Split ingenuo quebra");

        string[] lines =
        [
            "6;\"Cabo HDMI; 2m\";3;39,90;2026-01-20;Sudeste",
            "2;\"Monitor 27\"\", curvo\";1;1899,00;2026-01-16;Sudeste",
        ];

        foreach (string line in lines)
        {
            string[] naive = line.Split(';');
            IReadOnlyList<string> parsed = CsvLineParser.Split(line, ';');

            _logger.LogInformation("  linha: {Linha}", line);
            _logger.LogWarning("    Split(';'): {Campos} campos -> produto = \"{Produto}\"", naive.Length, naive[1]);
            _logger.LogInformation("    parser:     {Campos} campos -> produto = \"{Produto}\"", parsed.Count, parsed[1]);
        }

        _logger.LogInformation(
            "O separador dentro de aspas e o caso comum: endereco, descricao, nome de produto. O Split acha campos demais e desloca TODOS os seguintes.");
    }

    private void RunIncrementalPipeline(string path)
    {
        Section("5. Ler, transformar e escrever sem materializar");

        string outputPath = Path.Combine(SampleFileFactory.Directory, "resumo-sul.csv");
        CultureInfo brazil = CultureInfo.GetCultureInfo("pt-BR");

        long before = GC.GetTotalMemory(forceFullCollection: true);
        Stopwatch watch = Stopwatch.StartNew();

        int written = 0;
        decimal total = 0m;

        using (StreamWriter writer = new StreamWriter(outputPath, append: false, Encoding.UTF8))
        {
            writer.WriteLine("id;produto;total");

            // Entra uma linha, sai uma linha. Nenhuma colecao intermediaria.
            foreach (CsvRow row in StreamingCsvReader.ReadRows(path, brazil))
            {
                if (row.Record is not SaleRecord record || record.Region != "Sul")
                {
                    continue;
                }

                writer.WriteLine("{0};{1};{2}", record.Id, record.Product, record.Total.ToString("F2", brazil));
                total += record.Total;
                written++;
            }
        }

        long after = GC.GetTotalMemory(forceFullCollection: true);
        long elapsed = watch.ElapsedMilliseconds;
        long outputBytes = new FileInfo(outputPath).Length;

        _logger.LogInformation(
            "  {Escritos:N0} linhas da regiao Sul escritas em {Tempo}ms ({Tamanho:N1} MB de saida)",
            written,
            elapsed,
            outputBytes / 1024.0 / 1024.0);

        _logger.LogInformation("  total faturado: {Total}", total.ToString("C", brazil));

        _logger.LogInformation(
            "  memoria retida pelo pipeline inteiro: {Memoria} MB",
            FormatMegabytes(after - before));

        _logger.LogInformation(
            "Entrada de {Entrada:N0} linhas, saida de {Saida:N0}, e a memoria nao acompanha nenhuma das duas — e isso que permite processar arquivo maior que a RAM.",
            LargeFileRows,
            written);
    }

    /// <summary>
    /// Coleta de verdade antes de medir. Uma chamada só a <c>GC.GetTotalMemory</c> pode
    /// deixar para trás objetos recém-soltos, e a medição seguinte sai negativa.
    /// </summary>
    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Abaixo da resolução da medição, o número exato não significa nada — e um
    /// "-0,00 MB" impresso só confunde.
    /// </summary>
    private static string FormatMegabytes(long bytes)
    {
        double megabytes = bytes / 1024.0 / 1024.0;

        return Math.Abs(megabytes) < 0.1
            ? "~0,0"
            : megabytes.ToString("N1", CultureInfo.GetCultureInfo("pt-BR"));
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
