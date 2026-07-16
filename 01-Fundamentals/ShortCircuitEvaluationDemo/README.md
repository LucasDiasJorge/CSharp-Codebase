# ShortCircuitEvaluationDemo

Projeto didático em C# que demonstra o short-circuit evaluation (avaliação de curto-circuito) dos operadores condicionais `&&` e `||`, comparando-os com os operadores bitwise `&` e `|` (que sempre avaliam os dois lados).

## Visão geral

O objetivo do exemplo é tornar visível, por meio de métodos com efeito colateral (`Console.WriteLine`), se o lado direito de uma expressão condicional foi ou não avaliado. Cada seção do `Program.cs` executa uma expressão e imprime se o método do lado direito foi chamado, provando na prática o comportamento de curto-circuito.

Além do comportamento básico dos operadores, o projeto mostra por que o short-circuit é usado como guard clause em cenários reais: evitar `NullReferenceException`, `DivideByZeroException` e `IndexOutOfRangeException` ao encadear condições com `&&`. Cada guard clause é comparada com a mesma expressão usando `&`, que avalia os dois lados e lança a exceção correspondente. Por fim, o exemplo relaciona o conceito aos operadores `??` (null-coalescing) e `?.` (null-conditional), que também aplicam curto-circuito.

## Conceitos abordados

- Short-circuit evaluation nos operadores `&&` e `||`.
- Diferença de comportamento entre `&&`/`||` (com curto-circuito) e `&`/`|` (sem curto-circuito) aplicados a `bool`.
- Guard clauses com `&&` para evitar `NullReferenceException`, `DivideByZeroException` e `IndexOutOfRangeException`.
- Curto-circuito nos operadores `??` (null-coalescing) e `?.` (null-conditional).

## Objetivos de aprendizagem

- Entender quando o C# deixa de avaliar o restante de uma expressão condicional.
- Escrever guard clauses seguras usando a ordem correta das condições em `&&`.
- Reconhecer os riscos de usar `&`/`|` no lugar de `&&`/`||` em expressões booleanas.
- Relacionar o short-circuit de `&&`/`||` ao comportamento de `??` e `?.`.

## Estrutura do projeto

```text
ShortCircuitEvaluationDemo/
+-- Program.cs
`-- ShortCircuitEvaluationDemo.csproj
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/ShortCircuitEvaluationDemo/ShortCircuitEvaluationDemo.csproj
```

## Boas práticas e pontos de atenção

- Prefira sempre `&&`/`||` em vez de `&`/`|` para combinar condições booleanas; use `&`/`|` apenas para operações bitwise em tipos inteiros.
- Ordene as condições de um guard clause do mais genérico para o mais específico (ex.: checar `null` antes de acessar uma propriedade, checar limites antes de indexar um array).
- O bloco que força `&` nas seções de guard clause é proposital e lança exceção; ele existe apenas para evidenciar o risco, não é um padrão a ser seguido.

## Conteúdo complementar

| Operador | Curto-circuito? | Uso recomendado |
|----------|------------------|------------------|
| `&&` | Sim | Combinar condições booleanas em `if`, guard clauses |
| `\|\|` | Sim | Combinar condições booleanas alternativas |
| `&` | Não | Operações bitwise em `int`, `byte`, etc. |
| `\|` | Não | Operações bitwise em `int`, `byte`, etc. |
| `??` | Sim (lado direito só roda se o esquerdo for `null`) | Valor padrão para referência/nullable `null` |
| `?.` | Sim (membro só é acessado se a instância não for `null`) | Acesso seguro a membros de referências possivelmente nulas |

## Referências e documentação complementar

- [Conditional logical AND operator && - C# reference](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/boolean-logical-operators#conditional-logical-and-operator-)
- [Conditional logical OR operator || - C# reference](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/boolean-logical-operators#conditional-logical-or-operator-)
- Projeto relacionado: [`../LogicalOperatorsDemo`](../LogicalOperatorsDemo) - operadores lógicos e bitwise em geral.
