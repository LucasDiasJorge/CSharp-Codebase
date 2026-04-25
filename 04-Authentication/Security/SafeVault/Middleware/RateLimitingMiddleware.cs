namespace SafeVault.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IConfiguration _configuration;
    
    // Simple in-memory storage for rate limiting
    // In production, consider using a distributed cache like Redis
    private static readonly Dictionary<string, Queue<DateTime>> _requestStore = new();
    
    public RateLimitingMiddleware(
        RequestDelegate next, 
        ILogger<RateLimitingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint()?.DisplayName ?? "Unknown";
        var ipAddress = GetIpAddress(context);
        
        // Stricter limits for authentication endpoints
        var isAuthEndpoint = endpoint.Contains("Login") || endpoint.Contains("Register");
        
        // Different limits for different endpoint types
        int limit = isAuthEndpoint
            ? _configuration.GetValue<int>("SecuritySettings:RateLimits:LoginAttemptsPerMinute", 5)
            : _configuration.GetValue<int>("SecuritySettings:RateLimits:RequestsPerMinutePerIp", 100);
        
        // Create entry key - stricter limits per endpoint for auth, general limit for others
        string key = isAuthEndpoint
            ? $"{ipAddress}:{endpoint}"
            : ipAddress;
        
        lock (_requestStore)
        {
            // Initialize if this is the first request
            if (!_requestStore.TryGetValue(key, out var requestLog))
            {
                requestLog = new Queue<DateTime>();
                _requestStore[key] = requestLog;
            }
            
            // Clean up old requests outside the 1-minute window
            var now = DateTime.UtcNow;
            while (requestLog.Count > 0 && now.Subtract(requestLog.Peek()).TotalMinutes > 1)
            {
                requestLog.Dequeue();
            }
            
            // Check if limit exceeded
            if (requestLog.Count >= limit)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IpAddress} on endpoint: {Endpoint}", ipAddress, endpoint);
                
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Add("Retry-After", "60");
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests. Please try again later."
                });
                
                return;
            }
            
            // Add current request to log
            requestLog.Enqueue(now);
        }
        
        // Continue with the request
        await _next(context);
    }

    private string GetIpAddress(HttpContext context)
    {
        // Try to get the real IP if behind a proxy
        string ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
                    context.Connection.RemoteIpAddress?.ToString() ?? 
                    "Unknown";
        
        return ip;
    }
}
