using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using ScalarDocumentationSdk.Configuration;

namespace ScalarDocumentationSdk.Extensions;

public static class ScalarDocumentationServiceCollectionExtensions
{
    public static IServiceCollection AddScalarDocumentationSdk(
        this IServiceCollection services,
        Action<ScalarDocumentationSdkOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        ScalarDocumentationSdkOptions options = new ScalarDocumentationSdkOptions();
        configure?.Invoke(options);
        ValidateOptions(options);

        services.AddSingleton(options);
        services.AddOpenApi(options.DocumentName);

        return services;
    }

    private static void ValidateOptions(ScalarDocumentationSdkOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DocumentName))
        {
            throw new ArgumentException("DocumentName precisa ser informado.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.OpenApiRoutePattern))
        {
            throw new ArgumentException("OpenApiRoutePattern precisa ser informado.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.ScalarRoutePrefix))
        {
            throw new ArgumentException("ScalarRoutePrefix precisa ser informado.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Title))
        {
            throw new ArgumentException("Title precisa ser informado.", nameof(options));
        }
    }
}
