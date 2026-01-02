namespace TransactionScript.Examples.CreateInvoice.DTOs;

/// <summary>
/// Item da invoice na entrada
/// </summary>
public class InvoiceItemInput
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
