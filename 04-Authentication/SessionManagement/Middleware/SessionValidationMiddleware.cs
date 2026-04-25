using SessionManagement.Services;

namespace SessionManagement.Middleware;

/// <summary>
/// Validates that the JWT's embedded SessionId is still active in the database.
/// Runs after UseAuthentication() so context.User is already populated.
/// Also updates LastActivityAt for sliding-expiration tracking.
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string? sessionIdClaim = context.User.FindFirst("SessionId")?.Value;

            if (!Guid.TryParse(sessionIdClaim, out Guid sessionId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid session token" });
                return;
            }

            bool isValid = await sessionService.ValidateSessionAsync(sessionId);
            if (!isValid)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Session expired or revoked" });
                return;
            }

            await sessionService.UpdateLastActivityAsync(sessionId);
        }

        await _next(context);
    }
}
