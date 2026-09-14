using ApiVersioningDemo.Services;
using Asp.Versioning;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<CatalogStore>();

// Faz o erro de versao invalida responder application/problem+json em vez de um 400
// com corpo vazio. Detalhes de ProblemDetails no projeto ProblemDetailsApi.
builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // Requisicao sem versao cai na padrao. Conveniente para clientes antigos, mas
    // esconde de quem chama qual contrato ele esta consumindo de fato.
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Acrescenta api-supported-versions e api-deprecated-versions em toda resposta:
    // e assim que o cliente descobre que precisa migrar sem ler changelog.
    options.ReportApiVersions = true;

    // Combine aceita as tres formas ao mesmo tempo. Em producao normalmente se escolhe
    // uma; aqui as tres coexistem porque o objetivo e compara-las.
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version"));

    // Descontinuacao gradual (RFC 8594): a v1 responde com o header Sunset informando a
    // data de desligamento e um Link para o guia de migracao.
    options.Policies.Sunset(1.0)
        .Effective(new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero))
        .Link("https://example.com/guias/migracao-v2")
            .Title("Guia de migracao para a v2")
            .Type("text/html");
})
.AddMvc()
.AddApiExplorer(options =>
{
    // "'v'VVV" gera os nomes de grupo v1 e v2, usados como nome de documento OpenAPI.
    options.GroupNameFormat = "'v'VVV";

    // Substitui {version:apiVersion} pelo valor concreto na documentacao.
    options.SubstituteApiVersionInUrl = true;
})
// Integra o versionamento ao OpenAPI nativo do ASP.NET Core.
.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Um documento por versao: /openapi/v1.json e /openapi/v2.json. Sao esses
    // documentos que dizem ao cliente o que cada contrato promete.
    app.MapOpenApi().WithDocumentPerVersion();
}

app.MapControllers();

app.Run();
