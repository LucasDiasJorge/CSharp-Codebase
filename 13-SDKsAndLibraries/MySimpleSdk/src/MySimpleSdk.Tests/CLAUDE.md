# CLAUDE.md — MySimpleSdk.Tests

Testes xUnit do `MySimpleSdk`. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet test 13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Tests/MySimpleSdk.Tests.csproj
```

## Estrutura interna

- `Client/SdkClientTests.cs` — testes de `SdkClient`.
- `Services/SdkServiceTests.cs` — testes de `SdkService`, com Moq para as dependências.

## Pontos de atenção

- **BUILD QUEBRADO — falta o `ProjectReference`.** O `.csproj` não referencia `MySimpleSdk`, então os tipos testados não existem na compilação:

  ```
  error CS0246: O nome do tipo ou do namespace "SdkClient" não pode ser encontrado
  error CS0246: O nome do tipo ou do namespace "SdkService" não pode ser encontrado
  ```

  Correção:

  ```bash
  dotnet add 13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Tests/MySimpleSdk.Tests.csproj \
    reference 13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk/MySimpleSdk.csproj
  ```

  Esta é uma causa **diferente** da dos 10 projetos sem `TargetFramework` descritos no [CLAUDE.md](../../../../CLAUDE.md) raiz.
- **TFM `net5.0`**, fora de suporte. xUnit 2.4.1, Moq 4.16.1, `Microsoft.NET.Test.Sdk` 16.9.4 — o conjunto de teste mais antigo do repositório.
- **Sem README local**; documentação em `13-SDKsAndLibraries/MySimpleSdk/`.
