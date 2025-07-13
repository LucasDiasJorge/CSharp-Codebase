using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityAndAuthentication.Data;
using SecurityAndAuthentication.Models;

namespace SecurityAndAuthentication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
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
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "User registered successfully",
            userId = user.Id,
            username = user.Username,
            email = user.Email
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
            email = user.Email
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new 
            {
                u.Id,
                u.Username,
                u.Email,
                u.CreatedAt,
                u.IsSubscribed
            })
            .ToListAsync();

        return Ok(users);
    }

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
}
