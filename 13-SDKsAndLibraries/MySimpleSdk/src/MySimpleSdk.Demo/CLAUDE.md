# CLAUDE.md — MySimpleSdk.Demo

Console que consome `MySimpleSdk` do ponto de vista do usuário final. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Demo/MySimpleSdk.Demo.csproj
dotnet run --project 13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Demo/MySimpleSdk.Demo.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`) que referencia `MySimpleSdk` por `ProjectReference` e exercita a API pública. Serve como teste de ergonomia: o que estiver desconfortável aqui é problema de design do SDK, não do demo.

## Pontos de atenção

- **TFM `net5.0`** — fora de suporte desde 2022. O SDK referenciado é `netstandard2.0`, então a combinação funciona, mas o demo é o elo mais antigo do repositório junto com `MySimpleSdk.Tests`.
- **Sem README local**; documentação em `13-SDKsAndLibraries/MySimpleSdk/`.
