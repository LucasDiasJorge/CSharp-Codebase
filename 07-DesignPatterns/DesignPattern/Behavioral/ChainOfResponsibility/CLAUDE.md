# CLAUDE.md — Chain of Responsibility

Console: aprovação de despesas percorrendo uma cadeia de aprovadores. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Behavioral/ChainOfResponsibility/ChainOfResponsability.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/ChainOfResponsibility/ChainOfResponsability.csproj
```

**Atenção ao nome do arquivo**: a pasta é `ChainOfResponsibility`, mas o `.csproj` é `ChainOfResponsability.csproj` (grafia incorreta). Use exatamente como está — renomear exige atualizar o `.sln`.

## Estrutura interna

Arquivo único (`Program.cs`): cada aprovador tem um limite de alçada e uma referência ao próximo. Ao receber a despesa, trata ou repassa. O emissor não sabe quem vai decidir — é isso que o padrão compra.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Uma cadeia sem terminador engole a requisição silenciosamente. Ao estender, garanta que o último elo trate ou sinalize explicitamente.
