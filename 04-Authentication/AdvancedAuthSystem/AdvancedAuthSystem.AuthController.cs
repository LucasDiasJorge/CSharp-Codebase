using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdvancedAuthSystem.Services;
using AdvancedAuthSystem.DTOs.Auth;
using System.Security.Claims;

namespace AdvancedAuthSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, user, error) = await _authService.RegisterAsync(request);
        
        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "User registered successfully", userId = user!.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, user, error) = await _authService.AuthenticateAsync(request.Username, request.Password);
        
        if (!success)
            return Unauthorized(new { message = error });

        // Check if 2FA is enabled
        if (user!.TwoFactorEnabled)
        {
            var tempToken = _tokenService.GenerateTempToken(user.Id);
            return Ok(new TokenResponse(
                AccessToken: string.Empty,
                RefreshToken: string.Empty,
                ExpiresIn: 0,
                RequiresTwoFactor: true,
                TempToken: tempToken
            ));
        }

        // Generate tokens
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user, roles, user.UserClaims);
        var refreshToken = await _authService.CreateRefreshTokenAsync(user.Id);

        return Ok(new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken!.Token,
            ExpiresIn: 3600
        ));
    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> Verify2FA([FromBody] TwoFactorRequest request)
    {
        var userId = _tokenService.ValidateTempToken(request.TempToken);
        if (userId == null)
            return BadRequest(new { message = "Invalid or expired temp token" });

        var isValid = await _authService.Verify2FAAsync(userId.Value, request.Code);
        if (!isValid)
            return Unauthorized(new { message = "Invalid 2FA code" });

        // Re-authenticate to get user data
        var (success, user, _) = await _authService.AuthenticateAsync(request.Username, string.Empty);
        if (!success || user == null)
            return Unauthorized(new { message = "Authentication failed" });

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user, roles, user.UserClaims);
        var refreshToken = await _authService.CreateRefreshTokenAsync(user.Id);

        return Ok(new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken!.Token,
            ExpiresIn: 3600
        ));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var refreshToken = await _authService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (refreshToken == null)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        var user = refreshToken.User;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        
        var accessToken = _tokenService.GenerateAccessToken(user, roles, user.UserClaims);
        var newRefreshToken = await _authService.CreateRefreshTokenAsync(user.Id);
        
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken, "Refreshed");

        return Ok(new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken!.Token,
            ExpiresIn: 3600
        ));
    }

    [Authorize]
    [HttpPost("enable-2fa")]
    public async Task<IActionResult> Enable2FA()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, secret, qrCode, error) = await _authService.Enable2FAAsync(userId);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new Enable2FAResponse(
            Secret: secret!,
            QrCodeUrl: qrCode!,
            ManualEntryKey: secret!
        ));
    }

    [Authorize]
    [HttpPost("disable-2fa")]
    public async Task<IActionResult> Disable2FA()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _authService.Disable2FAAsync(userId);

        if (!success)
            return BadRequest(new { message = "Failed to disable 2FA" });

        return Ok(new { message = "2FA disabled successfully" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken, "User logout");
        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            username = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            claims
        });
    }
}
