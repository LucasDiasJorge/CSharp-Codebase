using System.Text.Json;
using OrderRuleConsole.Services;
using OrderRuleConsole.Models;

// Contract:
// - Input: JSON matching RuleInput
// - Behavior: Applies rule to an example Order (or provided via --order)
// - Output: Resulting Order as JSON

var argsList = args.ToList();
bool readFromStdIn = argsList.Contains("--stdin");
string? orderJson = null;
int opIndex = argsList.IndexOf("--order");
if (opIndex >= 0 && opIndex + 1 < argsList.Count)
{
	orderJson = argsList[opIndex + 1];
}

RuleInput input;
if (readFromStdIn)
{
	var raw = Console.In.ReadToEnd();
	input = JsonSerializer.Deserialize<RuleInput>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
			?? throw new InvalidOperationException("Invalid JSON for RuleInput");
}
else
{
	// Default sample from instructions
    var sample = """
    {"parameterKey":"OrderTypeCode","parameterValue":12,"operation":8,"ruleException":""}
    """;
	input = JsonSerializer.Deserialize<RuleInput>(sample, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
}

Order order;
if (!string.IsNullOrWhiteSpace(orderJson))
{
	order = JsonSerializer.Deserialize<Order>(orderJson!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
			  ?? new Order { Operation = 8, OrderTypeCode = 1 };
}
else
{
	// Example order
	order = new Order
	{
		Operation = 8,
		OrderTypeCode = 1
	};
}

var preOutput = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine(preOutput);

OrderRuleEngine.Apply(input, order);

var output = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine(output);
