using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth;

[ApiController]
[Route("api/example")]
[Authorize] // Requires authentication for all actions
public class SecureController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")] // Only users with "Admin" role can access
    public IActionResult AdminOnly()
    {
        return Ok("This is an admin-only endpoint.");
    }

    [HttpGet("user")]
    [Authorize(Roles = "User")]
    public IActionResult UserOrAdmin()
    {
        return Ok("This is a user endpoint.");
    }
}