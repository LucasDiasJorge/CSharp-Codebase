using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using ScalarDocumentationSdk.Configuration;

namespace ScalarDocumentationSdk.Extensions;

public static class ScalarDocumentationWebApplicationExtensions
{
    public static WebApplication MapScalarDocumentationSdk(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        ScalarDocumentationSdkOptions options = app.Services.GetRequiredService<ScalarDocumentationSdkOptions>();

        if (options.MapOnlyInDevelopment && !app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi(options.OpenApiRoutePattern);

        app.MapScalarApiReference(options.ScalarRoutePrefix, scalarOptions =>
        {
            scalarOptions
                .WithTitle(options.Title)
                .WithTheme(options.Theme)
                .WithOpenApiRoutePattern(options.OpenApiRoutePattern)
                .WithDefaultHttpClient(options.DefaultClientTarget, options.DefaultClient);
        });

        return app;
    }
}
