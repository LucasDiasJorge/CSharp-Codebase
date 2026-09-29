using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace ResilientHttpSdk;

/// <summary>
/// O único ponto de entrada do SDK na aplicação. Um SDK bem embalado oferece um
/// <c>Add*</c> e nada mais — o consumidor não deveria precisar saber que existe
/// <c>HttpClient</c>, handler ou política de retry por dentro.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Nome lógico do cliente. Constante porque aparece em log e em teste.</summary>
    public const string HttpClientName = "ResilientHttpSdk";

    public static IServiceCollection AddResilientHttpSdk(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ResilientHttpSdkOptions>()
            .Bind(configuration.GetSection(ResilientHttpSdkOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.AddResilientHttpSdkCore();
    }

    public static IServiceCollection AddResilientHttpSdk(
        this IServiceCollection services,
        Action<ResilientHttpSdkOptions> configure)
    {
        services
            .AddOptions<ResilientHttpSdkOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.AddResilientHttpSdkCore();
    }

    private static IServiceCollection AddResilientHttpSdkCore(this IServiceCollection services)
    {
        IHttpClientBuilder builder = services
            .AddHttpClient<IProductsClient, ProductsClient>(HttpClientName, (serviceProvider, client) =>
            {
                ResilientHttpSdkOptions options = serviceProvider
                    .GetRequiredService<IOptions<ResilientHttpSdkOptions>>()
                    .Value;

                // BaseAddress precisa terminar em '/': sem a barra, o ultimo segmento e
                // substituido em vez de concatenado, e "products/1" vira uma URL errada.
                client.BaseAddress = EnsureTrailingSlash(options.BaseAddress!);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                if (!string.IsNullOrEmpty(options.ApiKey))
                {
                    client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
                }
            });

        builder.AddResilienceHandler("sdk-retry", (pipeline, context) =>
        {
            ResilientHttpSdkOptions options = context.ServiceProvider
                .GetRequiredService<IOptions<ResilientHttpSdkOptions>>()
                .Value;

            // MaxRetryAttempts = 0 NAO e aceito por HttpRetryStrategyOptions (o minimo e 1,
            // e passar zero lanca ValidationException ao montar o pipeline). Desligar retry
            // significa nao adicionar a estrategia.
            if (options.MaxRetryAttempts == 0)
            {
                return;
            }

            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = options.MaxRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(options.BaseDelayMilliseconds),
                UseJitter = true,

                // A parte que mais se erra: o que MERECE nova tentativa. Repetir 400 ou
                // 404 nao muda o resultado e só multiplica carga; 401 tambem nao.
                ShouldHandle = arguments => ValueTask.FromResult(ShouldRetry(arguments.Outcome)),
            });
        });

        return services;
    }

    private static bool ShouldRetry(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is HttpRequestException)
        {
            return true;
        }

        if (outcome.Result is null)
        {
            return false;
        }

        HttpStatusCode status = outcome.Result.StatusCode;

        return status is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }

    private static Uri EnsureTrailingSlash(Uri baseAddress) =>
        baseAddress.AbsoluteUri.EndsWith('/') ? baseAddress : new Uri(baseAddress.AbsoluteUri + "/");
}
