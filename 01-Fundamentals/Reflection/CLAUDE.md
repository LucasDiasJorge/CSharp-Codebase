# CLAUDE.md — Reflection

Console de introspecção de tipos em runtime. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/Reflection/Reflection.csproj
dotnet run --project 01-Fundamentals/Reflection/Reflection.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): leitura de metadados (`Type`, `PropertyInfo`, `MethodInfo`), invocação dinâmica e instanciação via `Activator`. Os tipos inspecionados são declarados no próprio arquivo, para que metadado e origem fiquem visíveis juntos.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- `ImplicitUsings` está ativo; `System.Reflection` ainda assim precisa de `using` explícito.
