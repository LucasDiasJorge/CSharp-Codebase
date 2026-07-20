namespace HtmlToPdfAndTemplateEngine.Models;

public sealed class InvoiceItem
{
    public required string Description { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }

    public decimal Total => Quantity * UnitPrice;
}
