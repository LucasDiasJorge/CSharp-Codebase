namespace SafeVault.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers to help prevent various attacks
        
        // Prevents MIME type sniffing
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        
        // Controls how much information the browser includes with referrers
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Helps prevent XSS attacks
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        
        // Controls which features and APIs can be used in the browser
        context.Response.Headers.Add("Feature-Policy", 
            "accelerometer 'none'; camera 'none'; geolocation 'none'; gyroscope 'none'; " +
            "magnetometer 'none'; microphone 'none'; payment 'none'; usb 'none'");
        
        // Prevents clickjacking attacks
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        
        // Content-Security-Policy to restrict resources
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "form-action 'self'; " +
            "base-uri 'self'; " +
            "object-src 'none'");

        await _next(context);
    }
}
