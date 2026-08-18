# CLAUDE.md — ParameterObject

Console: lista longa de parâmetros substituída por um objeto nomeado. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/ParameterObject/ParameterObject.csproj
dotnet run --project 07-DesignPatterns/ParameterObject/ParameterObject.csproj
```

## Estrutura interna

Comparação direta, dois arquivos com a mesma responsabilidade:

- `GeradorContratoSemPattern.cs` — oito parâmetros posicionais. Trocar dois do mesmo tipo compila e produz resultado errado em silêncio.
- `GeradorContratoComPattern.cs` — recebe um `ContratoParametros`.
- `ContratoParametros.cs` — o objeto de parâmetros.

**Mantenha as duas versões.** Apagar a "sem pattern" elimina a comparação que é o conteúdo do projeto.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Aborda o mesmo problema que `DesignPattern/Creational/Builder`, por caminho diferente (objeto de parâmetros versus API fluente).
