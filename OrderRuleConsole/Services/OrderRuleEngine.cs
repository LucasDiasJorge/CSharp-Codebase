using System.Text.RegularExpressions;
using OrderRuleConsole.Models;

namespace OrderRuleConsole.Services;

public static class OrderRuleEngine
{
    // Applies rule to order based on input descriptor
    public static void Apply(RuleInput input, Order order)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (order == null) throw new ArgumentNullException(nameof(order));

        // Only support OrderTypeCode key for now
        if (!string.Equals(input.ParameterKey, nameof(Order.OrderTypeCode), StringComparison.OrdinalIgnoreCase))
            return;

        // If operation is 8, set OrderTypeCode to input.ParameterValue, unless RuleException blocks it
        if (order.Operation == input.Operation)
        {
            order.OrderTypeCode = input.ParameterValue;
        }
    }

}
