using Scalar.AspNetCore;

namespace ScalarDocumentationSdk.Configuration;

public sealed class ScalarDocumentationSdkOptions
{
    public string DocumentName { get; set; } = "v1";

    public string OpenApiRoutePattern { get; set; } = "/openapi/{documentName}.json";

    public string ScalarRoutePrefix { get; set; } = "/scalar";

    public string Title { get; set; } = "API Reference";

    public bool MapOnlyInDevelopment { get; set; } = true;

    public ScalarTheme Theme { get; set; } = ScalarTheme.BluePlanet;

    public ScalarTarget DefaultClientTarget { get; set; } = ScalarTarget.CSharp;

    public ScalarClient DefaultClient { get; set; } = ScalarClient.HttpClient;
}
