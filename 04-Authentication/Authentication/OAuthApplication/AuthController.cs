using OAuthApplication.Services;

namespace OAuthApplication;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenValidationService _tokenValidationService;

    public AuthController(IAuthService authService, ITokenValidationService tokenValidationService)
    {
        _tokenValidationService = tokenValidationService;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Username, request.Password);
        if (token == null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(new { access_token = token });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var success = await _authService.LogoutAsync(request.RefreshToken);
        if (!success)
            return BadRequest(new { message = "Logout failed" });

        return Ok(new { message = "Successfully logged out" });
    }
    
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] TokenValidationRequest request)
    {
        var isValid = await _tokenValidationService.ValidateTokenAsync(request.Token);
    
        if (!isValid)
            return Unauthorized(new { message = "Invalid or expired token" });

        return Ok(new { message = "Token is valid" });
    }

    public class TokenValidationRequest
    {
        public string Token { get; set; }
    }

}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class LogoutRequest
{
    public string RefreshToken { get; set; }
}
