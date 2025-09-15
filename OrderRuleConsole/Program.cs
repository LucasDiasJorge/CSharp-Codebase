using System.Text.Json;
using OrderRuleConsole.Services;
using OrderRuleConsole.Models;

namespace OrderRuleConsole;

internal static class Program
{
    public static int Main(string[] args)
    {
        RuleInput input = new RuleInput(
            // For operation 8 (set OrderName to "Lucas" if field == "Name")
            operation: 8,
            field: "OrderName",
            value: "Lucas",
            //if
            conditionField: "OrderTypeCode",
            comparison: "==",
            targetField: "1"
        );

        Order order = new Order { Operation = 8, OrderTypeCode = 1, OrderName = "Name" };

        string preOutput = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine("Pre rules application:");
        Console.WriteLine(preOutput);

        OrderRuleEngine.Apply(input, order);
        string output = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine("Post rules application:");
        Console.WriteLine(output);

        return 0;
    }

}
