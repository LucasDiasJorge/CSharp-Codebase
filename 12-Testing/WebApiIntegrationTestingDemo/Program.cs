using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WebApiIntegrationTestingDemo.Quotes;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Enum como string no JSON. Sem isto, Status sai como 0 e 1 — e o teste de contrato e
// o unico lugar onde isso aparece antes do cliente reclamar.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<QuoteRepository>();

// A dependencia externa. Em teste, esta linha e substituida via ConfigureTestServices.
builder.Services.AddSingleton<IExchangeRateProvider, RemoteExchangeRateProvider>();

WebApplication app = builder.Build();

// UseExceptionHandler() sem opcoes transforma QUALQUER excecao em 500 — inclusive a
// BadHttpRequestException que o binding lanca para JSON malformado, que deveria ser 400.
// O StatusCodeSelector preserva o status que a excecao ja carrega.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = exception => exception is BadHttpRequestException badRequest
        ? badRequest.StatusCode
        : StatusCodes.Status500InternalServerError,
});

app.UseStatusCodePages();

app.MapGet("/quotes/{id:guid}", (Guid id, QuoteRepository repository) =>
{
    QuoteResponse? quote = repository.Find(id);

    return quote is null
        ? Results.Problem(
            title: "cotacao nao encontrada",
            detail: $"nao existe cotacao com id {id}",
            statusCode: StatusCodes.Status404NotFound)
        : Results.Ok(quote);
});

app.MapPost("/quotes", (CreateQuoteRequest request, QuoteRepository repository, IExchangeRateProvider rates) =>
{
    // Validacao explicita: Minimal API nao valida DataAnnotations sozinha.
    List<ValidationResult> validationResults = new List<ValidationResult>();
    bool isValid = Validator.TryValidateObject(
        request,
        new ValidationContext(request),
        validationResults,
        validateAllProperties: true);

    if (!isValid)
    {
        Dictionary<string, string[]> errors = validationResults
            .GroupBy(result => result.MemberNames.FirstOrDefault() ?? "request")
            .ToDictionary(
                group => group.Key,
                group => group.Select(result => result.ErrorMessage ?? "invalido").ToArray());

        return Results.ValidationProblem(errors);
    }

    decimal unitPrice;

    try
    {
        unitPrice = rates.GetUnitPrice(request.Symbol);
    }
    catch (ExchangeRateUnavailableException exception)
    {
        // Dependencia externa indisponivel e 503, nao 500.
        return Results.Problem(
            title: "cotacao indisponivel",
            detail: exception.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    QuoteResponse quote = new QuoteResponse(
        Guid.NewGuid(),
        request.Symbol,
        request.Quantity,
        unitPrice,
        unitPrice * request.Quantity,
        QuoteStatus.Open,
        DateTimeOffset.UtcNow);

    repository.Add(quote);

    // 201 com Location: o cliente segue o header para buscar o recurso criado.
    return Results.Created($"/quotes/{quote.Id}", quote);
});

app.Run();

/// <summary>
/// Âncora de tipo para <c>WebApplicationFactory&lt;Program&gt;</c>. Com top-level
/// statements a classe gerada é interna, e sem esta declaração o projeto de teste não
/// consegue referenciá-la.
/// </summary>
public partial class Program;
