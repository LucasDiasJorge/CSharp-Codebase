using System.Globalization;
using System.Text;

namespace CsvStreamingProcessor.Demo;

/// <summary>
/// Gera os arquivos usados pelos cenários. Fica em <see cref="AppContext.BaseDirectory"/>
/// porque <c>dotnet run --project</c> mantém o diretório atual de quem chamou — caminho
/// relativo aqui espalharia arquivos pela raiz do repositório.
/// </summary>
public static class SampleFileFactory
{
    private static readonly string[] Products = ["Teclado", "Monitor 27\"", "Mouse", "Headset", "Webcam"];
    private static readonly string[] Regions = ["Sul", "Sudeste", "Norte", "Nordeste", "Centro-Oeste"];

    public static string Directory => Path.Combine(AppContext.BaseDirectory, "csv-demo");

    /// <summary>
    /// Escreve um arquivo grande de forma incremental — o gerador também não pode
    /// montar o conteúdo inteiro em memória.
    /// </summary>
    public static string CreateLargeFile(int rowCount)
    {
        System.IO.Directory.CreateDirectory(Directory);

        string path = Path.Combine(Directory, "vendas-grande.csv");
        CultureInfo brazil = new CultureInfo("pt-BR");
        Random random = new Random(20260923);

        using StreamWriter writer = new StreamWriter(path, append: false, Encoding.UTF8);

        writer.WriteLine("id;produto;quantidade;valor_unitario;data;regiao");

        for (int id = 1; id <= rowCount; id++)
        {
            string product = Products[random.Next(Products.Length)];
            int quantity = random.Next(1, 20);
            decimal price = Math.Round((decimal)(random.NextDouble() * 900 + 20), 2);
            DateOnly date = new DateOnly(2026, 1, 1).AddDays(random.Next(365));
            string region = Regions[random.Next(Regions.Length)];

            writer.WriteLine(
                "{0};{1};{2};{3};{4};{5}",
                id,
                Quote(product),
                quantity,
                price.ToString("F2", brazil),
                date.ToString("yyyy-MM-dd"),
                region);
        }

        return path;
    }

    /// <summary>
    /// Arquivo pequeno e sujo: campos a menos, número quebrado, quantidade zerada,
    /// separador dentro de campo entre aspas e aspa escapada.
    /// </summary>
    public static string CreateMessyFile()
    {
        System.IO.Directory.CreateDirectory(Directory);

        string path = Path.Combine(Directory, "vendas-sujo.csv");

        string[] lines =
        [
            "id;produto;quantidade;valor_unitario;data;regiao",
            "1;Teclado;2;129,90;2026-01-15;Sul",
            "2;\"Monitor 27\"\", curvo\";1;1899,00;2026-01-16;Sudeste",
            "3;Mouse;abc;49,90;2026-01-17;Norte",
            "4;Headset;1;2026-01-18;Nordeste",
            "5;Webcam;0;299,00;2026-01-19;Sul",
            "6;\"Cabo HDMI; 2m\";3;39,90;2026-01-20;Sudeste",
            "7;Notebook;1;7.499,90;2026-01-21;Sul",
        ];

        File.WriteAllLines(path, lines, Encoding.UTF8);

        return path;
    }

    public static void Cleanup()
    {
        if (System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.Delete(Directory, recursive: true);
        }
    }

    /// <summary>
    /// Só coloca aspas quando o campo precisa delas, e dobra as aspas internas.
    /// </summary>
    private static string Quote(string field)
    {
        bool needsQuotes = field.Contains(';') || field.Contains('"') || field.Contains('\n');

        return needsQuotes
            ? "\"" + field.Replace("\"", "\"\"") + "\""
            : field;
    }
}
