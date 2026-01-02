namespace TransactionScript.Examples.ProcessRefund.DTOs;

/// <summary>
/// Entrada do script de reembolso
/// </summary>
public class ProcessRefundInput
{
    public Guid AccountId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}
