using System.Globalization;
using HtmlToPdfAndTemplateEngine.Models;
using Scriban;

namespace HtmlToPdfAndTemplateEngine.Services;

public sealed class HtmlTemplateRenderer
{
    private static readonly CultureInfo Culture = new CultureInfo("pt-BR");

    public async Task<string> RenderInvoiceAsync(string templatePath, InvoiceData invoice)
    {
        string templateSource = await File.ReadAllTextAsync(templatePath);
        Template parsedTemplate = Template.Parse(templateSource, templatePath);

        if (parsedTemplate.HasErrors)
        {
            string errors = string.Join(Environment.NewLine, parsedTemplate.Messages);
            throw new InvalidOperationException($"Falha ao interpretar o template '{templatePath}': {errors}");
        }

        object viewModel = BuildViewModel(invoice);
        return await parsedTemplate.RenderAsync(viewModel);
    }

    private object BuildViewModel(InvoiceData invoice)
    {
        List<object> items = invoice.Items
            .Select(item => (object)new
            {
                item.Description,
                item.Quantity,
                UnitPrice = item.UnitPrice.ToString("C2", Culture),
                Total = item.Total.ToString("C2", Culture)
            })
            .ToList();

        return new
        {
            invoice.InvoiceNumber,
            invoice.CustomerName,
            IssueDate = invoice.IssueDate.ToString("dd/MM/yyyy", Culture),
            Items = items,
            Total = invoice.Total.ToString("C2", Culture)
        };
    }
}
