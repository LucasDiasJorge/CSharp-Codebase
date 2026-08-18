# CLAUDE.md — SymbolicDelegates

Console que constrói uma mini linguagem de script sobre um dicionário de delegates. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/SymbolicDelegates/SymbolicDelegates.csproj
dotnet run --project 01-Fundamentals/SymbolicDelegates/SymbolicDelegates.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`). A ideia: símbolos (strings) mapeados para delegates num dicionário formam uma tabela de despacho; um avaliador simples lê tokens e aplica o delegate correspondente. É como se obtém uma linguagem utilizável com quase nenhuma infraestrutura — sem parser gerado, sem AST formal.

## Pontos de atenção

- TFM **`net10.0`** (mais novo que a maioria da trilha, que está em `net9.0`). Preserve ao editar.
- Sem dependências externas.
