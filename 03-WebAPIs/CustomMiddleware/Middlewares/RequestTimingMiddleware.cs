using System.Diagnostics;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Add header before request processing
        context.Response.OnStarting(() => 
        {
            stopwatch.Stop();
            context.Response.Headers["X-Request-Duration"] = $"{stopwatch.ElapsedMilliseconds}ms";
            return Task.CompletedTask;
        });

        await _next(context);
        
        _logger.LogInformation("Request to {Path} took {ElapsedMs}ms", 
            context.Request.Path, stopwatch.ElapsedMilliseconds);
    }
}