using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApiIntegrationTestingDemo.Quotes;
using Xunit.Abstractions;

namespace WebApiIntegrationTestingDemo.Tests;

/// <summary>
/// Substituir dependências: o que funciona, o que não funciona, e por quê.
/// </summary>
public sealed class DependencySubstitutionTests
{
    private readonly ITestOutputHelper _output;

    public DependencySubstitutionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ConfigureTestServices_SubstituiODependenciaExterna()
    {
        using QuotesApiFactory factory = new QuotesApiFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/quotes",
            new { symbol = "PETR", quantity = 4 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // 38,50 e o preco do stub — prova que o provedor remoto nao foi usado.
        Assert.Equal(38.50m, body.RootElement.GetProperty("unitPrice").GetDecimal());
        Assert.Equal(1, factory.Rates.CallCount);
    }

    /// <summary>
    /// A crença difundida é que substituir em <c>ConfigureServices</c> "não funciona",
    /// porque ele rodaria <b>antes</b> das registros da aplicação — e o <c>RemoveAll</c>
    /// não teria o que remover.
    ///
    /// <b>Medido no .NET 10, isso não se reproduz</b>: os dois pontos de extensão são
    /// aplicados depois das registros da aplicação, e o resultado é idêntico — uma única
    /// implementação registrada, que é a do teste.
    ///
    /// <c>ConfigureTestServices</c> continua sendo o certo a usar, porque essa ordem é a
    /// garantia documentada dele; <c>ConfigureServices</c> apenas não falha como se conta.
    /// </summary>
    [Fact]
    public async Task ConfigureServices_E_ConfigureTestServices_TemOMesmoEfeitoNesteHost()
    {
        using ConfigureServicesApiFactory viaConfigureServices = new ConfigureServicesApiFactory();
        using QuotesApiFactory viaTestServices = new QuotesApiFactory();

        HttpResponseMessage first = await viaConfigureServices.CreateClient()
            .PostAsJsonAsync("/quotes", new { symbol = "PETR", quantity = 4 });

        HttpResponseMessage second = await viaTestServices.CreateClient()
            .PostAsJsonAsync("/quotes", new { symbol = "PETR", quantity = 4 });

        string[] viaConfigureServicesRegistrations = viaConfigureServices.Services
            .GetServices<IExchangeRateProvider>()
            .Select(provider => provider.GetType().Name)
            .ToArray();

        string[] viaTestServicesRegistrations = viaTestServices.Services
            .GetServices<IExchangeRateProvider>()
            .Select(provider => provider.GetType().Name)
            .ToArray();

        _output.WriteLine("ConfigureServices     -> " + string.Join(", ", viaConfigureServicesRegistrations));
        _output.WriteLine("ConfigureTestServices -> " + string.Join(", ", viaTestServicesRegistrations));

        // Os dois substituem de fato: o provedor remoto lancaria e a resposta seria 503.
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        // E os dois deixam uma unica implementacao: o RemoveAll funcionou nos dois casos.
        Assert.Equal(viaTestServicesRegistrations, viaConfigureServicesRegistrations);
        Assert.Equal(new[] { nameof(StubExchangeRateProvider) }, viaConfigureServicesRegistrations);
    }

    /// <summary>
    /// Em <c>ConfigureTestServices</c>, que roda <b>depois</b> das registros da
    /// aplicação, o <c>RemoveAll</c> faz o que promete: sobra uma única implementação.
    /// É o contraste com o teste anterior.
    /// </summary>
    [Fact]
    public void ConfigureTestServices_ComRemoveAll_DeixaApenasUmaImplementacao()
    {
        using QuotesApiFactory factory = new QuotesApiFactory();

        IServiceProvider services = factory.Services;
        IEnumerable<IExchangeRateProvider> all = services.GetServices<IExchangeRateProvider>();

        Assert.Single(all);
        Assert.IsType<StubExchangeRateProvider>(services.GetRequiredService<IExchangeRateProvider>());
    }

    /// <summary>
    /// Substituição por teste, sem fábrica dedicada: <c>WithWebHostBuilder</c> cria uma
    /// aplicação derivada com configuração extra.
    /// </summary>
    [Fact]
    public async Task WithWebHostBuilder_PermiteSubstituirSoNesteTeste()
    {
        using QuotesApiFactory factory = new QuotesApiFactory();

        HttpClient client = factory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IExchangeRateProvider>();
                services.AddSingleton<IExchangeRateProvider>(new FixedPriceProvider(99.99m));
            }))
            .CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/quotes", new { symbol = "PETR", quantity = 2 });

        JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(99.99m, body.RootElement.GetProperty("unitPrice").GetDecimal());
        Assert.Equal(199.98m, body.RootElement.GetProperty("total").GetDecimal());
    }

    private sealed class FixedPriceProvider : IExchangeRateProvider
    {
        private readonly decimal _price;

        public FixedPriceProvider(decimal price)
        {
            _price = price;
        }

        public decimal GetUnitPrice(string symbol) => _price;
    }
}

/// <summary>
/// Isolamento entre testes: uma instância da fábrica é uma instância da aplicação, com
/// o seu próprio repositório em memória.
/// </summary>
public sealed class IsolationTests
{
    private readonly ITestOutputHelper _output;

    public IsolationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task FabricasDiferentes_NaoCompartilhamEstado()
    {
        using QuotesApiFactory first = new QuotesApiFactory();
        using QuotesApiFactory second = new QuotesApiFactory();

        HttpResponseMessage created = await first.CreateClient()
            .PostAsJsonAsync("/quotes", new { symbol = "PETR", quantity = 1 });

        string location = created.Headers.Location!.ToString();

        // A mesma URL, na outra aplicacao, nao existe.
        HttpResponseMessage fromOther = await second.CreateClient().GetAsync(location);

        _output.WriteLine($"{location} na primeira: {(int)created.StatusCode}, na segunda: {(int)fromOther.StatusCode}");

        Assert.Equal(HttpStatusCode.NotFound, fromOther.StatusCode);
    }

    /// <summary>
    /// O outro lado: clientes da <b>mesma</b> fábrica falam com a mesma aplicação e
    /// compartilham o singleton. É a causa mais comum de teste que só passa sozinho.
    /// </summary>
    [Fact]
    public async Task ClientesDaMesmaFabrica_CompartilhamOSingleton()
    {
        using QuotesApiFactory factory = new QuotesApiFactory();

        HttpResponseMessage created = await factory.CreateClient()
            .PostAsJsonAsync("/quotes", new { symbol = "VALE", quantity = 1 });

        string location = created.Headers.Location!.ToString();

        // Outro HttpClient, mesma aplicacao: o recurso esta la.
        HttpResponseMessage fromAnotherClient = await factory.CreateClient().GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, fromAnotherClient.StatusCode);

        QuoteRepository repository = factory.Services.GetRequiredService<QuoteRepository>();

        Assert.True(repository.Count >= 1);
    }
}
