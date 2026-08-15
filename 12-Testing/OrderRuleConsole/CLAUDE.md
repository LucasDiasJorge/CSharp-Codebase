# CLAUDE.md — OrderRuleConsole

Console com motor de regras que aplica alterações a objetos `Order` em runtime. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 12-Testing/OrderRuleConsole/OrderRuleConsole.csproj
dotnet run --project 12-Testing/OrderRuleConsole/OrderRuleConsole.csproj

dotnet test 12-Testing/OrderRuleConsole/OrderRuleConsole.Tests/OrderRuleConsole.Tests.csproj
```

## Estrutura interna

- `Models/RuleInput.cs` — descrição declarativa da regra: condição, propriedade alvo e novo valor.
- `Services/OrderRuleEngine.cs` — o motor: avalia a condição e aplica a alteração no `Order`. É a classe coberta pelos testes.
- `Models/Order.cs` — o objeto manipulado.

**O projeto de testes vive dentro desta pasta** (`OrderRuleConsole.Tests/`), e o `.csproj` principal o exclui explicitamente da compilação via `<Compile Remove="OrderRuleConsole.Tests\**\*.cs" />`. Sem essa exclusão o build quebraria — preserve esse `ItemGroup` ao editar o `.csproj`.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- `Models/RuleInput.cs:38` gera warning **CS8618** (propriedade não anulável sem valor no construtor). É o único aviso do build; corrigir com `required` ou tipo anulável é uma limpeza legítima.
