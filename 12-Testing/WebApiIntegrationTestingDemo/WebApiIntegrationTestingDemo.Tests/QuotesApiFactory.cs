using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApiIntegrationTestingDemo.Quotes;

namespace WebApiIntegrationTestingDemo.Tests;

/// <summary>
/// Sobe a aplicação <b>de verdade</b> em memória: o mesmo <c>Program.cs</c>, o mesmo
/// pipeline, os mesmos serializadores e o mesmo tratamento de erro. O que muda é só a
/// dependência externa.
///
/// Não há porta TCP: o <c>HttpClient</c> devolvido despacha direto no pipeline.
/// </summary>
public sealed class QuotesApiFactory : WebApplicationFactory<Program>
{
    public StubExchangeRateProvider Rates { get; } = new StubExchangeRateProvider();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // ConfigureTestServices roda DEPOIS das registros da aplicacao — e por isso que
        // a substituicao vence. Com ConfigureServices (que roda antes), a aplicacao
        // registraria o provedor real depois e ganharia.
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IExchangeRateProvider>();
            services.AddSingleton<IExchangeRateProvider>(Rates);
        });
    }
}

/// <summary>
/// A mesma substituição, feita em <c>ConfigureServices</c> em vez de
/// <c>ConfigureTestServices</c>, <b>com</b> <c>RemoveAll</c>.
///
/// Serve para medir a diferença real entre os dois pontos de extensão: aqui o
/// <c>RemoveAll</c> não encontra nada para remover, porque as registros da aplicação
/// ainda não aconteceram. A registro do substituto sobrevive e ainda vence a resolução
/// simples — mas a da aplicação continua na coleção.
/// </summary>
public sealed class ConfigureServicesApiFactory : WebApplicationFactory<Program>
{
    public StubExchangeRateProvider Rates { get; } = new StubExchangeRateProvider();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IExchangeRateProvider>();
            services.AddSingleton<IExchangeRateProvider>(Rates);
        });
    }
}
