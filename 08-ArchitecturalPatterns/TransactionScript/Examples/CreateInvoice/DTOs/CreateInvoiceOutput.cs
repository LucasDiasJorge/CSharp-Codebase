namespace TransactionScript.Examples.CreateInvoice.DTOs;

/// <summary>
/// Saída do script de criação de invoice
/// </summary>
public class CreateInvoiceOutput
{
    public Guid InvoiceId { get; set; }
    public int InvoiceNumber { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public DateTime DueDate { get; set; }
}
