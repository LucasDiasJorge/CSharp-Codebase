namespace TransactionScript.Examples.ProcessRefund.DTOs;

/// <summary>
/// Saída do script de reembolso
/// </summary>
public class ProcessRefundOutput
{
    public Guid RefundId { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal NewAccountBalance { get; set; }
    public int NewProductStock { get; set; }
    public DateTime ProcessedAt { get; set; }
}
