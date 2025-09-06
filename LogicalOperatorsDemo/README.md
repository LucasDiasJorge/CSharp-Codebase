# LogicalOperatorsDemo

Projeto didático em C# que demonstra todos os operadores lógicos básicos:
- AND (&&)
- OR (||)
- NOT (!)
- EXCLUSIVE (XOR) (^)

Também mostra operadores bitwise para comparar comportamento em inteiros: `&`, `|`, `^`, `~`.

Código em `Program.cs` usa tipos explícitos (sem `var`) e imprime tabelas verdade para as operações.

Como rodar (PowerShell):
```powershell
dotnet run --project .\LogicalOperatorsDemo\LogicalOperatorsDemo.csproj
```

O programa imprime as tabelas de verdade e exemplos comentados.

Explicação rápida:
- AND (A && B) é true apenas quando ambos são true.
- OR (A || B) é true quando pelo menos um é true.
- NOT (!A) inverte o valor booleano.
- XOR (A ^ B) é true quando exatamente um dos operandos é true.

Bitwise vs Logical:
- Operadores lógicos atuam em booleanos e fazem short-circuit (&&, ||).
- Operadores bitwise atuam em inteiros (bits) e não fazem short-circuit.

Este projeto é intencionalmente pequeno para focar na didática.
