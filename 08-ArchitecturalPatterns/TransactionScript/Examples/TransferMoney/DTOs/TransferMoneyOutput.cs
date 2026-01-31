namespace TransactionScript.Examples.TransferMoney.DTOs;

/// <summary>
/// Saída do script de transferência
/// </summary>
public class TransferMoneyOutput
{
    public Guid TransactionId { get; set; }
    public decimal FromAccountNewBalance { get; set; }
    public decimal ToAccountNewBalance { get; set; }
    public DateTime TransactionDate { get; set; }
}
