using HtmlToPdfAndTemplateEngine.Models;
using HtmlToPdfAndTemplateEngine.Services;

string projectRoot = FindProjectRoot(AppContext.BaseDirectory);
string templatePath = Path.Combine(projectRoot, "Templates", "invoice-template.html");
string outputPath = Path.Combine(projectRoot, "Output", "invoice.pdf");

InvoiceData invoice = new InvoiceData
{
    InvoiceNumber = "NF-2026-0007",
    IssueDate = DateOnly.FromDateTime(DateTime.Now),
    CustomerName = "Empresa Exemplo Ltda.",
    Items = new List<InvoiceItem>
    {
        new InvoiceItem { Description = "Consultoria em arquitetura de software", Quantity = 10, UnitPrice = 250m },
        new InvoiceItem { Description = "Licenca de uso - modulo de relatorios", Quantity = 1, UnitPrice = 1200m },
        new InvoiceItem { Description = "Suporte tecnico mensal", Quantity = 3, UnitPrice = 400m }
    }
};

HtmlTemplateRenderer renderer = new HtmlTemplateRenderer();
string html = await renderer.RenderInvoiceAsync(templatePath, invoice);

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
Console.WriteLine("Template renderizado a partir de dados dinamicos.");
Console.WriteLine($"Gerando PDF em: {outputPath}");

PdfConverter converter = new PdfConverter();

try
{
    await converter.ConvertHtmlToPdfAsync(html, outputPath);
    Console.WriteLine("PDF gerado com sucesso.");
}
catch (Exception ex)
{
    Console.WriteLine($"Nao foi possivel gerar o PDF (Chromium indisponivel neste ambiente): {ex.Message}");
    string htmlFallbackPath = Path.ChangeExtension(outputPath, ".html");
    await File.WriteAllTextAsync(htmlFallbackPath, html);
    Console.WriteLine($"HTML renderizado salvo como alternativa em: {htmlFallbackPath}");
}

static string FindProjectRoot(string startDirectory)
{
    DirectoryInfo? directory = new DirectoryInfo(startDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "HtmlToPdfAndTemplateEngine.csproj")))
    {
        directory = directory.Parent;
    }

    return directory?.FullName ?? startDirectory;
}
