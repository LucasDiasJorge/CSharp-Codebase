# CLAUDE.md

## Comando de build

```bash
dotnet build 13-SDKsAndLibraries/ScalarDocumentationSdk/src/ScalarDocumentationSdk/ScalarDocumentationSdk.csproj
```

## Visão da arquitetura

- `Configuration/ScalarDocumentationSdkOptions.cs`: modelo de opções para document name, rotas e comportamento de ambiente.
- `Extensions/ScalarDocumentationServiceCollectionExtensions.cs`: registro de opções e OpenAPI no container.
- `Extensions/ScalarDocumentationWebApplicationExtensions.cs`: mapeamento de OpenAPI e Scalar no pipeline da API.

## Pontos de atenção

- O SDK depende de `Scalar.AspNetCore` e `Microsoft.AspNetCore.OpenApi`.
- `MapOnlyInDevelopment` é habilitado por padrão para reduzir exposição em produção.
- Mudanças de contrato público das opções devem ser versionadas para manter compatibilidade.
