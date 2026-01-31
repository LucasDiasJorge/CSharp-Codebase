namespace TransactionScript.Examples.TransferMoney.DTOs;

/// <summary>
/// Entrada do script de transferência
/// </summary>
public class TransferMoneyInput
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
