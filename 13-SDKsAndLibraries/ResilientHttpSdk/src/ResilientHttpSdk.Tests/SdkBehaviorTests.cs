using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace ResilientHttpSdk.Tests;

/// <summary>
/// Tradução de erro: cada status vira um tipo de exceção do SDK, e o chamador nunca
/// precisa olhar código HTTP.
/// </summary>
public sealed class ErrorTranslationTests
{
    private const string ProductJson = """{"id":"p-1","name":"Teclado","price":129.90}""";

    [Fact]
    public async Task Sucesso_DevolveOProdutoDesserializado()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(HttpStatusCode.OK, ProductJson);
        using ServiceProvider provider = SdkTestHost.Build(handler);

        Product product = await provider.Client().GetAsync("p-1");

        Assert.Equal("p-1", product.Id);
        Assert.Equal("Teclado", product.Name);
        Assert.Equal(129.90m, product.Price);
    }

    [Fact]
    public async Task Status404_ViraSdkNotFoundException()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler()
            .Respond(HttpStatusCode.NotFound, """{"title":"nao encontrado","detail":"produto p-9 nao existe"}""");

        using ServiceProvider provider = SdkTestHost.Build(handler);

        SdkNotFoundException error = await Assert.ThrowsAsync<SdkNotFoundException>(
            () => provider.Client().GetAsync("p-9"));

        Assert.Equal("produto p-9 nao existe", error.Message);
        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
    }

    [Fact]
    public async Task Status400_ViraSdkValidationExceptionComErrosPorCampo()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(
            HttpStatusCode.BadRequest,
            """{"title":"invalido","detail":"corpo invalido","errors":{"Name":["obrigatorio"],"Price":["deve ser positivo"]}}""");

        using ServiceProvider provider = SdkTestHost.Build(handler);

        SdkValidationException error = await Assert.ThrowsAsync<SdkValidationException>(
            () => provider.Client().CreateAsync(new CreateProductRequest(string.Empty, -1m)));

        Assert.Equal(2, error.Errors.Count);
        Assert.Equal(new[] { "obrigatorio" }, error.Errors["Name"]);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task Status401E403_ViramSdkAuthenticationException(HttpStatusCode status)
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(status, """{"detail":"chave invalida"}""");
        using ServiceProvider provider = SdkTestHost.Build(handler);

        SdkAuthenticationException error = await Assert.ThrowsAsync<SdkAuthenticationException>(
            () => provider.Client().GetAsync("p-1"));

        Assert.Equal(status, error.StatusCode);
    }

    [Fact]
    public async Task Status429_CarregaORetryAfter()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(
            HttpStatusCode.TooManyRequests,
            """{"detail":"limite excedido"}""",
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30)));

        using ServiceProvider provider = SdkTestHost.Build(handler, options => options.MaxRetryAttempts = 0);

        SdkRateLimitException error = await Assert.ThrowsAsync<SdkRateLimitException>(
            () => provider.Client().GetAsync("p-1"));

        Assert.Equal(TimeSpan.FromSeconds(30), error.RetryAfter);
    }

    [Fact]
    public async Task FalhaDeTransporte_ViraSdkTransportException()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler()
            .Throw(new HttpRequestException("nao foi possivel resolver o host"));

        using ServiceProvider provider = SdkTestHost.Build(handler, options => options.MaxRetryAttempts = 0);

        SdkTransportException error = await Assert.ThrowsAsync<SdkTransportException>(
            () => provider.Client().GetAsync("p-1"));

        Assert.Null(error.StatusCode);
        Assert.IsType<HttpRequestException>(error.InnerException);
    }

    /// <summary>
    /// Corpo que não é JSON é normal em erro de gateway (HTML de proxy). O SDK não pode
    /// quebrar ao tentar interpretar isso como <c>ProblemDetails</c>.
    /// </summary>
    [Fact]
    public async Task CorpoDeErroQueNaoEJson_NaoQuebraATraducao()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler()
            .Respond(HttpStatusCode.BadGateway, "<html><body>502 Bad Gateway</body></html>");

        using ServiceProvider provider = SdkTestHost.Build(handler, options => options.MaxRetryAttempts = 0);

        SdkServerException error = await Assert.ThrowsAsync<SdkServerException>(
            () => provider.Client().GetAsync("p-1"));

        Assert.Contains("502", error.Message);
    }

    [Fact]
    public async Task ChaveDeApi_VaiNoHeaderDeTodaRequisicao()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(HttpStatusCode.OK, ProductJson);
        using ServiceProvider provider = SdkTestHost.Build(handler);

        await provider.Client().GetAsync("p-1");

        Assert.Equal("chave-de-teste", handler.Requests[0].Headers.GetValues("X-Api-Key").Single());
    }

    /// <summary>
    /// <c>BaseAddress</c> sem barra no fim faz o último segmento ser substituído. O SDK
    /// acrescenta a barra por isso.
    /// </summary>
    [Fact]
    public async Task BaseAddressSemBarra_AindaMontaAUrlCorreta()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(HttpStatusCode.OK, ProductJson);

        using ServiceProvider provider = SdkTestHost.Build(
            handler,
            options => options.BaseAddress = new Uri("https://api.exemplo.com/v2"));

        await provider.Client().GetAsync("p-1");

        Assert.Equal("https://api.exemplo.com/v2/products/p-1", handler.Requests[0].RequestUri?.ToString());
    }
}

/// <summary>
/// Retry: em que situações tentar de novo, e em quais não.
/// </summary>
public sealed class RetryTests
{
    private const string ProductJson = """{"id":"p-1","name":"Teclado","price":129.90}""";

    private readonly ITestOutputHelper _output;

    public RetryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Status503SeguidoDe200_TentaDeNovoEDevolveOProduto()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler()
            .Respond(HttpStatusCode.ServiceUnavailable)
            .Respond(HttpStatusCode.OK, ProductJson);

        using ServiceProvider provider = SdkTestHost.Build(handler);

        Product product = await provider.Client().GetAsync("p-1");

        _output.WriteLine($"tentativas: {handler.CallCount}");

        Assert.Equal("p-1", product.Id);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task Status503Persistente_TentaOConfiguradoEDepoisFalha()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(HttpStatusCode.ServiceUnavailable);

        using ServiceProvider provider = SdkTestHost.Build(handler, options => options.MaxRetryAttempts = 3);

        await Assert.ThrowsAsync<SdkServerException>(() => provider.Client().GetAsync("p-1"));

        _output.WriteLine($"tentativas: {handler.CallCount} (1 original + 3 retries)");

        Assert.Equal(4, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task ErroDoCliente_NaoEReteontado(HttpStatusCode status)
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(status, """{"detail":"nao insista"}""");

        using ServiceProvider provider = SdkTestHost.Build(handler);

        await Assert.ThrowsAnyAsync<SdkException>(() => provider.Client().GetAsync("p-1"));

        // Repetir 400/404/401 nao muda o resultado — so multiplica carga.
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task MaxRetryAttemptsZero_DesligaORetry()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Respond(HttpStatusCode.ServiceUnavailable);

        using ServiceProvider provider = SdkTestHost.Build(handler, options => options.MaxRetryAttempts = 0);

        await Assert.ThrowsAsync<SdkServerException>(() => provider.Client().GetAsync("p-1"));

        Assert.Equal(1, handler.CallCount);
    }

    /// <summary>
    /// Retry em <c>POST</c> reenvia a criação. O SDK não sabe se a primeira chegou, então
    /// isso é decisão de quem configura — e o exemplo mostra o risco em vez de esconder.
    /// </summary>
    [Fact]
    public async Task RetryEmPost_ReenviaACriacao()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler()
            .Respond(HttpStatusCode.ServiceUnavailable)
            .Respond(HttpStatusCode.OK, ProductJson);

        using ServiceProvider provider = SdkTestHost.Build(handler);

        await provider.Client().CreateAsync(new CreateProductRequest("Teclado", 129.90m));

        _output.WriteLine($"POSTs enviados: {handler.CallCount} — sem chave de idempotencia, o servidor pode criar dois");

        Assert.Equal(2, handler.CallCount);
        Assert.All(handler.Requests, request => Assert.Equal(HttpMethod.Post, request.Method));
    }
}

/// <summary>
/// Cancelamento e timeout: a mesma exceção, dois significados.
/// </summary>
public sealed class CancellationTests
{
    [Fact]
    public async Task CancelamentoDoChamador_PropagaOperationCanceledException()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Delay(TimeSpan.FromSeconds(30));
        using ServiceProvider provider = SdkTestHost.Build(handler);

        using CancellationTokenSource cancellation = new CancellationTokenSource();

        await cancellation.CancelAsync();

        // O chamador desistiu: precisa receber cancelamento, NAO erro do SDK.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.Client().GetAsync("p-1", cancellation.Token));
    }

    [Fact]
    public async Task TimeoutInterno_ViraSdkTransportException()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler().Delay(TimeSpan.FromSeconds(10));

        using ServiceProvider provider = SdkTestHost.Build(
            handler,
            options =>
            {
                options.TimeoutSeconds = 1;
                options.MaxRetryAttempts = 0;
            });

        // O token do chamador nao foi cancelado: quem estourou foi o timeout.
        SdkTransportException error = await Assert.ThrowsAsync<SdkTransportException>(
            () => provider.Client().GetAsync("p-1", CancellationToken.None));

        Assert.Contains("tempo limite", error.Message);
    }
}

/// <summary>
/// Configuração: validada no start, não no primeiro uso.
/// </summary>
public sealed class OptionsValidationTests
{
    [Fact]
    public void SemBaseAddress_FalhaNaValidacao()
    {
        ServiceCollection services = new ServiceCollection();

        services.AddResilientHttpSdk(options => options.TimeoutSeconds = 5);

        using ServiceProvider provider = services.BuildServiceProvider();

        OptionsValidationException error = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<ResilientHttpSdkOptions>>().Value);

        Assert.Contains("BaseAddress e obrigatorio", error.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(200)]
    public void TimeoutForaDaFaixa_FalhaNaValidacao(int timeoutSeconds)
    {
        ServiceCollection services = new ServiceCollection();

        services.AddResilientHttpSdk(options =>
        {
            options.BaseAddress = new Uri("https://api.exemplo.com");
            options.TimeoutSeconds = timeoutSeconds;
        });

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<ResilientHttpSdkOptions>>().Value);
    }

    [Fact]
    public void ConfiguracaoValida_EAceita()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler();
        using ServiceProvider provider = SdkTestHost.Build(handler);

        ResilientHttpSdkOptions options = provider.Options();

        Assert.Equal(new Uri("https://api.exemplo.com"), options.BaseAddress);
        Assert.Equal(3, options.MaxRetryAttempts);
    }
}
