# ScalarDocumentationSdk

## Visão geral

`ScalarDocumentationSdk` é um SDK para padronizar a configuração do Scalar em APIs ASP.NET Core com OpenAPI.

Ele centraliza a configuração em dois extension methods: um para registro no `IServiceCollection` e outro para mapeamento dos endpoints no `WebApplication`. O objetivo é reduzir código repetido e facilitar adoção consistente em múltiplos serviços.

## Conceitos abordados

- Encapsulamento de configuração de documentação em SDK reutilizável.
- Extension methods para DI e pipeline de endpoints.
- OpenAPI com `Microsoft.AspNetCore.OpenApi`.
- API Reference com `Scalar.AspNetCore`.
- Configuração por opções fortemente tipadas.

## Objetivos de aprendizagem

- Criar um SDK focado em produtividade para APIs.
- Padronizar configuração de OpenAPI e Scalar entre projetos.
- Reduzir acoplamento de configuração no `Program.cs`.
- Demonstrar integração real em um projeto arquitetural do repositório.

## Estrutura do projeto

```text
ScalarDocumentationSdk/
|-- src/
|   `-- ScalarDocumentationSdk/
|       |-- Configuration/
|       |   `-- ScalarDocumentationSdkOptions.cs
|       |-- Extensions/
|       |   |-- ScalarDocumentationServiceCollectionExtensions.cs
|       |   `-- ScalarDocumentationWebApplicationExtensions.cs
|       `-- ScalarDocumentationSdk.csproj
`-- README.md
```

## Como executar

Build do SDK:

```bash
dotnet build 13-SDKsAndLibraries/ScalarDocumentationSdk/src/ScalarDocumentationSdk/ScalarDocumentationSdk.csproj
```

Empacotar para uso interno:

```bash
dotnet pack 13-SDKsAndLibraries/ScalarDocumentationSdk/src/ScalarDocumentationSdk/ScalarDocumentationSdk.csproj -c Release
```

Integração prática no projeto arquitetural:

```bash
dotnet run --project 08-ArchitecturalPatterns/CountryRulesTimeProviderDemo/CountryRulesTimeProviderDemo.csproj
```

## Boas práticas e pontos de atenção

- Use `MapOnlyInDevelopment=true` para evitar exposição de documentação em produção por padrão.
- Padronize `DocumentName` e `OpenApiRoutePattern` em toda a organização para simplificar observabilidade e automação.
- Prefira centralizar ajustes visuais do Scalar no SDK, evitando replicação em cada API.
- Caso precise comportamento avançado por API, exponha novas opções no SDK em vez de duplicar configuração local.

## Conteúdo complementar

Exemplo mínimo no `Program.cs` de uma API:

```csharp
using ScalarDocumentationSdk.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddScalarDocumentationSdk(options =>
{
    options.DocumentName = "v1";
    options.Title = "Orders API";
    options.ScalarRoutePrefix = "/scalar";
    options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    options.MapOnlyInDevelopment = true;
});

WebApplication app = builder.Build();
app.MapScalarDocumentationSdk();
app.Run();
```

Rotas geradas com a configuração padrão:

- `GET /openapi/v1.json`
- `GET /scalar`

## Referências e documentação complementar

- [Scalar para ASP.NET Core](https://scalar.com/scalar/scalar-api-references/integrations/net-aspnet-core)
- [Microsoft.AspNetCore.OpenApi](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview)
- [Minimal APIs no ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
