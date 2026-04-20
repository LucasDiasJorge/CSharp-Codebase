# LogicalOperatorsDemo

## Visão geral

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

## Conceitos abordados

- Exemplo didático sobre LogicalOperatorsDemo no contexto de conceitos fundamentais da linguagem C# e orientação a objetos.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como LogicalOperatorsDemo se aplica em um cenário prático de conceitos fundamentais da linguagem C# e orientação a objetos.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
LogicalOperatorsDemo/
+-- LogicalOperatorsDemo.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/LogicalOperatorsDemo/LogicalOperatorsDemo.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.
