# OrderRuleConsole

Console app para aplicar regras simples a objetos `Order` em tempo de execução.

O projeto contém um motor de regras leve (`OrderRuleEngine`) que recebe uma descrição (`RuleInput`) e aplica alterações em propriedades alvo de um `Order` quando condições específicas são satisfeitas.

**Principais conceitos**
- `Order`: modelo de domínio que contém as propriedades que podem ser alteradas (ex.: `Operation`, `OrderTypeCode`, `OrderName`).
- `RuleInput`: contrato que descreve qual propriedade alterar, qual valor aplicar, a operação que ativa a regra e uma condição opcional (exceção) para evitar a mudança.
- `OrderRuleEngine`: implementa a lógica de resolução de propriedades por caminho, avaliação de condições simples e conversão/atribuição de valores.

**Comportamento resumido da regra**
- A regra só é aplicada se `order.Operation == rule.Operation`.
- Se `rule.ParameterKey` estiver vazio, a regra é ignorada.
- A propriedade alvo é resolvida por nome (suporta paths como `Parent.Child.Prop`).
- Antes de sobrescrever, o mecanismo avalia uma condição opcional (`RuleException`) que pode usar:
  - `ExceptionParameterKey`: comparar com outra propriedade do `Order` (ex.: `OrderName`).
  - `ExceptionParameterValue`: comparar com um literal.
  - Operadores suportados: `==` e `!=`.
- Se a condição falhar, a alteração é bloqueada; caso contrário, converte e atribui o `ParameterValue` ao tipo da propriedade alvo.

**Como rodar (PowerShell)**
- Abrir PowerShell e navegar até a pasta do projeto:

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\OrderRuleConsole"
```

- Rodar o projeto (usa a entrada fixa em `Program.cs` quando nenhuma entrada por stdin é fornecida):

```powershell
dotnet run
```

- Fornecer um `RuleInput` via stdin e um `Order` customizado (exemplo):

```powershell
$rule = '{"parameterKey":"OrderTypeCode","parameterValue":12,"operation":8,"ruleException":"!= 1"}'
$order = '{"operation":8,"orderTypeCode":1}'
$rule | dotnet run -- --stdin --order $order
```

**Estrutura do projeto**
- `Program.cs` — exemplo de execução e ponto de entrada.
- `Models/Order.cs` — definição do modelo `Order`.
- `Models/RuleInput.cs` — definição do contrato `RuleInput` (inclui construtor opcional).
- `Services/OrderRuleEngine.cs` — lógica principal para aplicar regras.

**Exemplos**
- Cenário simples (sem condição):

Entrada (RuleInput):
```json
{"parameterKey":"OrderTypeCode","parameterValue":12,"operation":8,"ruleException":""}
```
Order antes:
```json
{"operation":8,"orderTypeCode":5}
```
Order depois:
```json
{"operation":8,"orderTypeCode":12}
```

- Cenário com condição por literal (`ExceptionParameterValue`):

RuleInput:
```json
{"parameterKey":"OrderTypeCode","parameterValue":15,"operation":8,"ruleException":"==","exceptionParameterValue":"Name"}
```
Se `Order.OrderName` for igual a `"Name"`, a condição `==` faz com que a alteração seja permitida (ou bloqueada dependendo da lógica). O motor suporta comparar valores primitivos e strings (case-insensitive para strings).

**Detalhes técnicos**
- Resolução de propriedades é feita via reflexão (`BindingFlags.IgnoreCase`) e suporta caminhos dot-separated.
- Conversões de tipos usam `TypeDescriptor.GetConverter` e `Convert.ChangeType` como fallback.
- Comparações com `==`/`!=` tentam converter valores para o mesmo tipo antes de comparar; para strings a comparação é case-insensitive.
- Operadores desconhecidos na condição são tratados como permissivos (não bloqueiam a alteração).

**Boas práticas e limitações**
- Atualmente o motor assume que propriedades intermediárias no path já estão inicializadas (não cria objetos faltantes).
- Suporta apenas operadores simples (`==`, `!=`). Operadores lógicos compostos ou expressões complexas não são suportados.
- Conversão de tipos é conservadora; falhas causam exceção ao converter o `ParameterValue` para o tipo alvo.

**Sugestões de extensão**
- Adicionar suporte a mais operadores e expressões (por exemplo `>`, `<`, `>=`, `<=`, `contains`).
- Permitir funções de transformação (ex.: mapeamentos, formatação) antes de atribuir.
- Suporte a caminhos que criam instâncias intermediárias quando nulos.
- Registros de auditoria para mudanças aplicadas (quem/quando/por quê).
- Validadores pluggable por propriedade ou por tipo.

**Contribuindo**
- Abra uma issue descrevendo a proposta ou bug.
- Envie PRs pequenos e focados.

**Licença**
- Projeto sem licença especificada — defina uma `LICENSE` se desejar tornar o projeto público.

---

Se quiser, eu posso:
- Ajustar este README para incluir exemplos reais do `Program.cs` atual.
- Gerar testes unitários para `OrderRuleEngine`.
- Reverter o construtor adicionado em `RuleInput` para um factory se preferir.

Diga qual opção prefere que eu execute a seguir.
