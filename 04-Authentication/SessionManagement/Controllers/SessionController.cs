using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SessionManagement.DTOs;
using SessionManagement.Services;

namespace SessionManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    // POST api/session/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        (bool success, string? error) = await _sessionService.RegisterAsync(request);
        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "User registered successfully" });
    }

    // POST api/session/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        (bool success, AuthResponse? response, string? error) = await _sessionService.LoginAsync(request, ipAddress, userAgent);
        if (!success)
            return Unauthorized(new { message = error });

        return Ok(response);
    }

    // POST api/session/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        (bool success, AuthResponse? response, string? error) = await _sessionService.RefreshAsync(request.RefreshToken, ipAddress);
        if (!success)
            return Unauthorized(new { message = error });

        return Ok(response);
    }

    // POST api/session/logout  — revoke the current session
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        int userId = GetCurrentUserId();
        Guid? sessionId = GetCurrentSessionId();

        if (sessionId is null)
            return BadRequest(new { message = "No active session found in token" });

        await _sessionService.RevokeSessionAsync(sessionId.Value, userId, "User logout");
        return Ok(new { message = "Logged out successfully" });
    }

    // GET api/session  — list active sessions for the current user
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetActiveSessions()
    {
        int userId = GetCurrentUserId();
        Guid sessionId = GetCurrentSessionId() ?? Guid.Empty;

        List<SessionInfo> sessions = await _sessionService.GetActiveSessionsAsync(userId, sessionId);
        return Ok(sessions);
    }

    // DELETE api/session/{sessionId}  — revoke a specific session owned by the current user
    [Authorize]
    [HttpDelete("{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId, [FromBody] RevokeSessionRequest? request)
    {
        int userId = GetCurrentUserId();
        string reason = request?.Reason ?? "Revoked by user";

        bool success = await _sessionService.RevokeSessionAsync(sessionId, userId, reason);
        if (!success)
            return NotFound(new { message = "Session not found or not owned by current user" });

        return Ok(new { message = "Session revoked successfully" });
    }

    // DELETE api/session?keepCurrent=true  — revoke all sessions (optionally keep current)
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RevokeAllSessions([FromQuery] bool keepCurrent = true)
    {
        int userId = GetCurrentUserId();
        Guid? currentSessionId = keepCurrent ? GetCurrentSessionId() : null;

        await _sessionService.RevokeAllSessionsAsync(userId, "Revoked all sessions by user", currentSessionId);

        string message = keepCurrent ? "All other sessions revoked" : "All sessions revoked";
        return Ok(new { message });
    }

    // ------------------------------------------------------------------ helpers

    private int GetCurrentUserId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out int id) ? id : 0;
    }

    private Guid? GetCurrentSessionId()
    {
        string? value = User.FindFirst("SessionId")?.Value;
        return Guid.TryParse(value, out Guid sessionId) ? sessionId : null;
    }
}
