using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace WebApiIntegrationTestingDemo.Tests;

/// <summary>
/// O contrato HTTP: código de status, header, formato do corpo. Nada disso aparece em
/// teste de unidade do handler — ali o retorno é um objeto, não uma resposta HTTP.
/// </summary>
public sealed class HttpContractTests : IClassFixture<QuotesApiFactory>
{
    private readonly QuotesApiFactory _factory;
    private readonly ITestOutputHelper _output;

    public HttpContractTests(QuotesApiFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task Post_Valido_Retorna201ComLocationQueFunciona()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage created = await client.PostAsJsonAsync(
            "/quotes",
            new { symbol = "PETR", quantity = 10 });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        _output.WriteLine("Location: " + created.Headers.Location);

        // O contrato nao e "existe um header Location": e que seguir o header funciona.
        HttpResponseMessage fetched = await client.GetAsync(created.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);

        JsonDocument body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());

        Assert.Equal("PETR", body.RootElement.GetProperty("symbol").GetString());
        Assert.Equal(385.00m, body.RootElement.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Resposta_UsaCamelCaseEEnumComoString()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/quotes",
            new { symbol = "VALE", quantity = 2 });

        string json = await response.Content.ReadAsStringAsync();

        _output.WriteLine(json);

        // As propriedades do record sao PascalCase; o JSON sai camelCase.
        Assert.Contains("\"unitPrice\"", json);
        Assert.DoesNotContain("\"UnitPrice\"", json);

        // Enum como string, por causa do JsonStringEnumConverter no Program.cs.
        // Sem ele, aqui viria "status":0 — e o cliente quebraria em silencio.
        Assert.Contains("\"status\":\"Open\"", json);
    }

    /// <summary>
    /// As chaves do dicionário de erros saem em <b>PascalCase</b>, não em camelCase: a
    /// política de nomes do serializador vale para propriedades, não para chaves de
    /// dicionário. Mudar isso exige <c>DictionaryKeyPolicy</c>.
    /// </summary>
    [Theory]
    [InlineData("pe", 10, "Symbol")]
    [InlineData("PETROLEO", 10, "Symbol")]
    [InlineData("PETR", 0, "Quantity")]
    [InlineData("PETR", 5000, "Quantity")]
    public async Task Post_Invalido_Retorna400ComProblemDetails(string symbol, int quantity, string expectedField)
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/quotes", new { symbol, quantity });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.True(body.RootElement.TryGetProperty("errors", out JsonElement errors));
        Assert.True(errors.TryGetProperty(expectedField, out _), $"esperava erro no campo '{expectedField}'");
    }

    [Fact]
    public async Task Get_Inexistente_Retorna404ComProblemDetails()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync($"/quotes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(404, body.RootElement.GetProperty("status").GetInt32());
    }

    /// <summary>
    /// A restrição de rota <c>{id:guid}</c> faz o roteamento rejeitar antes do handler.
    /// O resultado é 404, não 400 — e isso é decisão de contrato que só se vê aqui.
    /// </summary>
    [Fact]
    public async Task Get_ComIdQueNaoEGuid_Retorna404PorRestricaoDeRota()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/quotes/nao-e-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComContentTypeErrado_Retorna415()
    {
        HttpClient client = _factory.CreateClient();

        StringContent content = new StringContent("symbol=PETR&quantity=10", Encoding.UTF8, "application/x-www-form-urlencoded");

        HttpResponseMessage response = await client.PostAsync("/quotes", content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task MetodoErradoNaRota_Retorna405()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.DeleteAsync("/quotes/" + Guid.NewGuid());

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComJsonMalformado_Retorna400()
    {
        HttpClient client = _factory.CreateClient();

        StringContent content = new StringContent("{\"symbol\": \"PETR\",", Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/quotes", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
