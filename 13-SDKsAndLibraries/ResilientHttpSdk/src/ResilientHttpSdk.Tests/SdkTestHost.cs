using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace ResilientHttpSdk.Tests;

/// <summary>
/// Monta o SDK como uma aplicação montaria, mas com o handler de teste no fim do
/// pipeline. O pipeline de resiliência continua real — é ele que está sendo testado.
/// </summary>
public static class SdkTestHost
{
    public static ServiceProvider Build(
        StubHttpMessageHandler handler,
        Action<ResilientHttpSdkOptions>? configure = null)
    {
        ServiceCollection services = new ServiceCollection();

        services.AddResilientHttpSdk(options =>
        {
            options.BaseAddress = new Uri("https://api.exemplo.com");
            options.ApiKey = "chave-de-teste";
            options.TimeoutSeconds = 5;
            options.MaxRetryAttempts = 3;

            // Backoff minimo: o teste nao pode esperar segundos por tentativa.
            options.BaseDelayMilliseconds = 1;

            configure?.Invoke(options);
        });

        // Substitui apenas o handler primario — o do fim da cadeia, que faria a rede.
        services.Configure<HttpClientFactoryOptions>(
            ServiceCollectionExtensions.HttpClientName,
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = handler));

        return services.BuildServiceProvider();
    }

    public static IProductsClient Client(this ServiceProvider provider) =>
        provider.GetRequiredService<IProductsClient>();

    public static ResilientHttpSdkOptions Options(this ServiceProvider provider) =>
        provider.GetRequiredService<IOptions<ResilientHttpSdkOptions>>().Value;
}
