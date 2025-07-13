using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityAndAuthentication.Data;
using SecurityAndAuthentication.Models;
using SecurityAndAuthentication.Services;

namespace SecurityAndAuthentication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;

    public AuthController(ApplicationDbContext context, AuthService authService)
    {
        _authService = authService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar se email já existe
        if (await _context.Users.AnyAsync(u => u.Email == registrationDto.Email))
        {
            return BadRequest(new { message = "Email already exists" });
        }

        // Verificar se username já existe
        if (await _context.Users.AnyAsync(u => u.Username == registrationDto.Username))
        {
            return BadRequest(new { message = "Username already exists" });
        }

        // Criar novo usuário
        var user = new User
        {
            Email = registrationDto.Email,
            Username = registrationDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password),
            IsSubscribed = registrationDto.Subscribe,
            Role = "User", // Default role for new users
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "User registered successfully",
            userId = user.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Buscar usuário por email ou username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.EmailOrUsername || 
                                    u.Username == loginDto.EmailOrUsername);

        if (user == null)
        {
            return BadRequest(new { message = "Invalid credentials" });
        }

        // Verificar senha
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return BadRequest(new { message = "Invalid credentials" });
        }

        return Ok(new 
        { 
            message = "Login successful",
            userId = user.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role,
            token = _authService.GenerateJwtToken(user.Username, user.Id, user.Role)
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new 
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt,
                u.IsSubscribed
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize]
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new 
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt,
                u.IsSubscribed
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationDto tokenDto)
    {
        if (string.IsNullOrEmpty(tokenDto.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        if (!_authService.ValidateToken(tokenDto.Token, out var principal))
        {
            return BadRequest(new { message = "Invalid or expired token" });
        }

        var userIdClaim = _authService.GetUserIdFromToken(tokenDto.Token);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest(new { message = "Invalid token claims" });
        }

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new 
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt,
                u.IsSubscribed
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        return Ok(new 
        { 
            valid = true,
            user = user,
            expiresAt = _authService.IsTokenExpired(tokenDto.Token) ? "Expired" : "Valid"
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenValidationDto tokenDto)
    {
        if (string.IsNullOrEmpty(tokenDto.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var userIdClaim = _authService.GetUserIdFromToken(tokenDto.Token);
        var usernameClaim = _authService.GetUsernameFromToken(tokenDto.Token);

        if (userIdClaim == null || usernameClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest(new { message = "Invalid token" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        // Gerar novo token
        var newToken = _authService.GenerateJwtToken(user.Username, user.Id, user.Role);

        return Ok(new
        {
            token = newToken,
            userId = user.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(assignRoleDto.UserId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Validate role
        var validRoles = new[] { "User", "Admin", "Moderator" };
        if (!validRoles.Contains(assignRoleDto.Role))
        {
            return BadRequest(new { message = "Invalid role. Valid roles are: User, Admin, Moderator" });
        }

        user.Role = assignRoleDto.Role;
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"Role '{assignRoleDto.Role}' assigned to user '{user.Username}' successfully",
            userId = user.Id,
            username = user.Username,
            newRole = user.Role
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("roles")]
    public IActionResult GetAvailableRoles()
    {
        var roles = new[] { "User", "Admin", "Moderator" };
        return Ok(new { roles });
    }
}
