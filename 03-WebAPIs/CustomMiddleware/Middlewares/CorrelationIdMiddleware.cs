namespace CustomMiddleware.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request headers
        if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Store in HttpContext for later use
        context.Items["CorrelationId"] = correlationId.ToString();

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append("X-Correlation-ID", correlationId.ToString());
            return Task.CompletedTask;
        });

        await _next(context);
    }
}