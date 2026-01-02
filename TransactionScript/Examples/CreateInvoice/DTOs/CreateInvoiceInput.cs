namespace TransactionScript.Examples.CreateInvoice.DTOs;

/// <summary>
/// Entrada do script de criação de invoice
/// </summary>
public class CreateInvoiceInput
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<InvoiceItemInput> Items { get; set; } = [];
    public int DueDays { get; set; } = 30;
    public decimal TaxRate { get; set; } = 0.18m; // 18% padrão
}
