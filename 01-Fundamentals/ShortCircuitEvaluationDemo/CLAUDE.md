# CLAUDE.md — ShortCircuitEvaluationDemo

Console que torna **visível** a avaliação de curto-circuito, comparando `&&`/`||` com `&`/`|`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/ShortCircuitEvaluationDemo/ShortCircuitEvaluationDemo.csproj
dotnet run --project 01-Fundamentals/ShortCircuitEvaluationDemo/ShortCircuitEvaluationDemo.csproj
```

## Estrutura interna

A técnica central: os operandos do lado direito são **métodos com efeito colateral** (`Console.WriteLine`). Se a linha do lado direito não aparece na saída, ele não foi avaliado — a prova é observável, não explicada em comentário. `Program.cs` aplica isso a `&&`/`||` versus `&`/`|`, guard clauses e aos operadores `??` e `?.`.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- **Não remova os efeitos colaterais dos métodos auxiliares**: eles são o instrumento de medição do exemplo, não código acidental.
- Complementar a `LogicalOperatorsDemo` (tabelas-verdade). Mantenha os escopos separados.
