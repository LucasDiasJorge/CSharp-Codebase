namespace HtmlToPdfAndTemplateEngine.Models;

public sealed class InvoiceData
{
    public required string InvoiceNumber { get; init; }
    public required DateOnly IssueDate { get; init; }
    public required string CustomerName { get; init; }
    public required List<InvoiceItem> Items { get; init; }

    public decimal Total => Items.Sum(item => item.Total);
}
