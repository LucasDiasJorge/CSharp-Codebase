# CLAUDE.md — Events

Console sobre `event`, `delegate` e o padrão publisher/subscriber. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/Events/Events.csproj
dotnet run --project 01-Fundamentals/Events/Events.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`) com publishers e subscribers declarados no mesmo escopo, para que assinatura (`+=`), disparo e cancelamento (`-=`) fiquem visíveis lado a lado. A ausência de separação em arquivos é deliberada.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Relacionado a `SymbolicDelegates` (delegates como valores em dicionário) — os dois cobrem faces diferentes do mesmo mecanismo.
