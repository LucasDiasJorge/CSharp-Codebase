namespace SafeVault.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(
        RequestDelegate next, 
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start timer
        var startTime = DateTime.UtcNow;
        
        // Capture request info
        var endpoint = context.GetEndpoint()?.DisplayName ?? "Unknown";
        var method = context.Request.Method;
        var path = context.Request.Path;
        var query = RedactSensitiveQueryData(context.Request.QueryString.ToString());
        
        // Get IP address
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
                      context.Connection.RemoteIpAddress?.ToString() ?? 
                      "Unknown";
        
        // Create correlation ID for tracking
        var correlationId = Guid.NewGuid().ToString();
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        try
        {
            // Log the request - careful not to log sensitive data
            _logger.LogInformation(
                "Request {CorrelationId}: {Method} {Path}{Query} from {IpAddress} to {Endpoint}",
                correlationId, method, path, query, ipAddress, endpoint);
            
            await _next(context);
            
            // Log the response
            var statusCode = context.Response.StatusCode;
            var elapsed = DateTime.UtcNow - startTime;
            
            // Log differently based on status code
            if (statusCode >= 500)
            {
                _logger.LogError(
                    "Response {CorrelationId}: {StatusCode} after {ElapsedMs}ms",
                    correlationId, statusCode, elapsed.TotalMilliseconds);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Response {CorrelationId}: {StatusCode} after {ElapsedMs}ms",
                    correlationId, statusCode, elapsed.TotalMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "Response {CorrelationId}: {StatusCode} after {ElapsedMs}ms",
                    correlationId, statusCode, elapsed.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Request {CorrelationId} failed: {ErrorMessage}",
                correlationId, ex.Message);
            
            throw; // Let the exception middleware handle it
        }
    }

    private string RedactSensitiveQueryData(string query)
    {
        // Replace sensitive data with asterisks
        var sensitiveParams = new[] { "password", "token", "auth", "key", "secret", "credential" };
        
        if (string.IsNullOrEmpty(query))
            return string.Empty;
        
        var result = query;
        foreach (var param in sensitiveParams)
        {
            // Look for the parameter and redact its value
            var regex = new System.Text.RegularExpressions.Regex($"{param}=([^&]+)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = regex.Replace(result, $"{param}=*****");
        }
        
        return result;
    }
}
