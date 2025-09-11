# OrderRuleConsole

Console app that applies the following rule to an Order:

- If `operation == 8`, set `OrderTypeCode` to `parameterValue`, regardless of current value.
- If `ruleException` is provided, support simple filters `== X` or `!= X` against the current `OrderTypeCode`:
  - If it matches, the change is skipped.

## Run (PowerShell)

- Default sample (uses embedded RuleInput and a sample Order):

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\OrderRuleConsole"
dotnet run
```

- Provide RuleInput via stdin and a custom order via `--order`:

```powershell
$rule = '{"parameterKey":"OrderTypeCode","parameterValue":12,"operation":8,"ruleException":"!= 1"}'
$order = '{"operation":8,"orderTypeCode":1}'
$rule | dotnet run -- --stdin --order $order
```

Output is the transformed Order as JSON.

## Sample

Input (RuleInput):
```json
{"parameterKey":"OrderTypeCode","parameterValue":12,"operation":8,"ruleException":""}
```

Order before:
```json
{"operation":8,"orderTypeCode":5}
```

Order after:
```json
{"operation":8,"orderTypeCode":12}
```

## Limitations

- Currently only supports the `OrderTypeCode` key and integer comparisons in `ruleException`.
- Extend `OrderRuleEngine` to support more keys and operators as needed.
