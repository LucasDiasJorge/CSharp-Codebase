namespace OrderRuleConsole.Models;

public class RuleInput
{
    public string ParameterKey { get; set; } = string.Empty; // e.g., "OrderTypeCode"
    public string ParameterValue { get; set; } // e.g., 12
    public int Operation { get; set; } // e.g., 8
    public string RuleException { get; set; } // operator: ==, !=
    public string? ExceptionParameterKey { get; set; } // optional secondary property key for comparative condition
    public string? ExceptionParameterValue { get; set; } // optional literal value to compare when key not provided

    /// <summary>
    /// Initializes a new instance of the RuleInput class with explicit parameter names.
    /// </summary>
    /// <param name="operation">The operation code.</param>
    /// <param name="field">The field to apply the rule to.</param>
    /// <param name="value">The value to compare or assign.</param>
    /// <param name="conditionField">The condition for the rule (e.g., "if" expression).</param>
    /// <param name="comparison">The comparison operator (e.g., "==", "!=", "&gt;", "&lt;").</param>
    /// <param name="targetField">The target field for the rule.</param>
    public RuleInput(
        int operation,
        string field,
        string value,
        string conditionField,
        string comparison,
        string targetField
    )
    {
        Operation = operation;
        ParameterKey = field;
        ParameterValue = value;
        RuleException = comparison;
        ExceptionParameterKey = conditionField;
        ExceptionParameterValue = targetField;
    }

    public RuleInput(int operation, string parameterKey, string parameterValue)
    {
        Operation = operation;
        ParameterKey = parameterKey ?? string.Empty;
        ParameterValue = parameterValue;
    }
}
