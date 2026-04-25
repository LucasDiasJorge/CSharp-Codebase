using Microsoft.Extensions.Options;

public class CustomHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CustomHeaderOptions _options;

    public CustomHeaderMiddleware(RequestDelegate next, IOptions<CustomHeaderOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add headers to the response
        foreach (var header in _options.Headers)
        {
            context.Response.Headers.Append(header.Key, header.Value);
        }

        await _next(context);
    }
}

public class CustomHeaderOptions
{
    public Dictionary<string, string> Headers { get; set; } = new();
}