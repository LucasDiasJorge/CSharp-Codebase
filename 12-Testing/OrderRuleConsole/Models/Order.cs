namespace OrderRuleConsole.Models;

public class Order
{
    public int Operation { get; set; }
    public int OrderTypeCode { get; set; }
    public string OrderName { get; set; } = string.Empty;

    // Add other fields here if needed
}
