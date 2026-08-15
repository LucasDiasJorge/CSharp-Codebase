# CLAUDE.md — MinimalApiDemo

Minimal API com EF Core InMemory e endpoints de métricas/observabilidade. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/MinimalApiDemo/MinimalApiDemo.csproj
dotnet run --project 03-WebAPIs/MinimalApiDemo/MinimalApiDemo.csproj
```

Requisições de exemplo em `requests.http`.

## Estrutura interna

- `Program.cs` — mapeamento dos endpoints direto no host, sem controllers. É o contraste com `SimpleWebAPI` (controllers) e `VerticalSliceMinimalApi` (fatias verticais).
- `ApplicationDbContext.cs` — EF Core **InMemory**: os dados somem a cada restart, por design.
- `Models/Product.cs` e `Annotations/ProductPriceAttribute.cs` — validação por atributo customizado.

## Pontos de atenção

- TFM `net9.0`.
- Traz `App.Metrics.AspNetCore` 4.3.0 e `Steeltoe.Management.EndpointCore` 3.2.8 — bibliotecas de observabilidade pesadas para um demo introdutório, e o Steeltoe 3.2.8 é bem anterior ao .NET 9. Se houver falha de build ou de startup neste projeto, comece por essas duas dependências.
- É o projeto citado como referência de execução no README raiz; mantenha-o funcionando.
