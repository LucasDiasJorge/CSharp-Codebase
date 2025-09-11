namespace OrderRuleConsole.Models;

public class RuleInput
{
    public string ParameterKey { get; set; } = string.Empty; // e.g., "OrderTypeCode"
    public int ParameterValue { get; set; } // e.g., 12
    public int Operation { get; set; } // e.g., 8
    public string RuleException { get; set; } = string.Empty; // e.g., "== 5" or "!= 7" or empty
}
