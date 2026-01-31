using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Models.DTOs;
using SafeVault.Services;

namespace SafeVault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var (success, message) = await _authService.RegisterAsync(registrationDto);
        
        if (!success)
        {
            return BadRequest(new { message });
        }
        
        return Ok(new { message });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var (success, token, message) = await _authService.LoginAsync(loginDto);
        
        if (!success)
        {
            // For security, don't distinguish between non-existent users and wrong passwords
            return Unauthorized(new { message = "Invalid credentials" });
        }
        
        return Ok(new { token });
    }

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    [HttpPost("validate-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationDto tokenDto)
    {
        if (string.IsNullOrEmpty(tokenDto.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }
        
        var isValid = await _authService.ValidateTokenAsync(tokenDto.Token);
        
        if (!isValid)
        {
            return Unauthorized(new { message = "Invalid or expired token" });
        }
        
        var userId = _authService.GetUserIdFromToken(tokenDto.Token);
        var role = _authService.GetRoleFromToken(tokenDto.Token);
        
        return Ok(new { valid = true, userId, role });
    }

    /// <summary>
    /// Changes a user's password
    /// </summary>
    [Authorize]
    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated properly" });
        }
        
        var success = await _authService.ChangePasswordAsync(userId, changePasswordDto);
        
        if (!success)
        {
            return BadRequest(new { message = "Failed to change password. Please check your current password." });
        }
        
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Assigns a role to a user (admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("assign-role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated properly" });
        }
        
        var success = await _authService.AssignRoleAsync(assignRoleDto, userId);
        
        if (!success)
        {
            return BadRequest(new { message = "Failed to assign role" });
        }
        
        return Ok(new { message = "Role assigned successfully" });
    }
}

// Define TokenValidationDto if it doesn't exist yet
public class TokenValidationDto
{
    public string Token { get; set; } = string.Empty;
}
