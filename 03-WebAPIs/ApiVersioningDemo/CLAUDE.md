# CLAUDE.md — ApiVersioningDemo

API com duas versões que demonstra seleção por URL, header e query string, mais descontinuação gradual com política de sunset. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/ApiVersioningDemo/ApiVersioningDemo.csproj
dotnet run --project 03-WebAPIs/ApiVersioningDemo/ApiVersioningDemo.csproj
```

Sobe em `http://localhost:5114`. Requisições prontas em `ApiVersioningDemo.http`.

## Estrutura interna

Dois recursos, de propósito com estratégias diferentes: `Controllers/V{1,2}/ProductsController` usa `[Route("api/v{version:apiVersion}/products")]` (versão na URL) e `Controllers/V{1,2}/OrdersController` usa `[Route("api/orders")]` (versão só por header ou query string). Os controllers têm o **mesmo nome de classe em namespaces por versão** — sufixar a classe (`OrdersV1Controller`) faz o ApiExplorer gerar a tag truncada `OrdersV` no OpenAPI.

`Services/CatalogStore` tem uma representação interna única e projeta cada contrato a partir dela. É a separação que mantém o versionamento sustentável: o domínio não é versionado, o contrato é.

`Versioning/RequestedVersionDescriber` existe para mostrar onde a versão resolvida mora — `IApiVersioningFeature`, não um método de extensão. **Atenção:** `HttpContext.GetRequestedApiVersion()` não existe mais na série 10.x; use `context.Features.Get<IApiVersioningFeature>()`.

Toda a configuração está em `Program.cs`: `ApiVersionReader.Combine` com os três readers, `ReportApiVersions`, e `options.Policies.Sunset(1.0)` com data e link.

## Pontos de atenção

- TFM `net10.0` (a maior parte da trilha é `net9.0`): a máquina não tem o runtime ASP.NET Core 9.0 instalado, e este sample precisa ser executado. Mesma decisão de [BlazorHelloWorld](../BlazorHelloWorld/CLAUDE.md).
- Pacotes: `Asp.Versioning.Mvc.ApiExplorer` 10.2.1 e `Asp.Versioning.OpenApi` 10.2.3, além de `Microsoft.AspNetCore.OpenApi` 10.0.11.
- **Wiring correto na 10.x** (os analisadores do pacote avisam se sair disso): `AddApiVersioning(...).AddMvc().AddApiExplorer(...).AddOpenApi()` e `app.MapOpenApi().WithDocumentPerVersion()`. Registrar `AddOpenApi("v1")`/`AddOpenApi("v2")` manualmente dispara AV0029; omitir `WithDocumentPerVersion` dispara AV0030.
- **Warning AV0016 é esperado e proposital.** `AssumeDefaultVersionWhenUnspecified = true` só se justifica em API legada, e o analisador diz isso. A opção fica ligada para demonstrar o fallback; o aviso está explicado no README. Não "conserte" removendo a opção, ou o cenário da rota sem versão deixa de existir.
- `AddProblemDetails()` está registrado só para dar corpo ao erro de versão inválida. Sem ele o 400 vem vazio. Aprofundamento em [ProblemDetailsApi](../ProblemDetailsApi/CLAUDE.md).
- A política de sunset se propaga sozinha para a descrição do documento OpenAPI da v1 — não duplique esse texto manualmente.
- As datas de sunset estão fixas em 31/12/2026. Se a data passar, o exemplo continua correto (o header vira uma data no passado), mas vale atualizar para o cenário seguir realista.
- **Fronteira com os vizinhos**: nenhum outro projeto da trilha versiona API. `SwaggerClientCode` trata de geração de cliente a partir do OpenAPI, não de versionamento.
