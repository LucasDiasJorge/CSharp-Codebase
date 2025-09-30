using Microsoft.IdentityModel.Tokens;
using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Models.DTOs;
using SafeVault.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SafeVault.Services;

public interface IAuthService
{
    Task<(bool Success, string? Token, string? Message)> LoginAsync(UserLoginDto loginDto);
    Task<(bool Success, string? Message)> RegisterAsync(UserRegistrationDto registrationDto);
    Task<bool> ValidateTokenAsync(string token);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
    Task<bool> AssignRoleAsync(AssignRoleDto assignRoleDto, int currentUserId);
    int? GetUserIdFromToken(string token);
    string? GetRoleFromToken(string token);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IInputValidator _inputValidator;
    private readonly IConfiguration _configuration;
    
    public AuthService(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher, 
        IInputValidator inputValidator,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _inputValidator = inputValidator;
        _configuration = configuration;
    }

    public async Task<(bool Success, string? Token, string? Message)> LoginAsync(UserLoginDto loginDto)
    {
        // Validate input for security
        if (string.IsNullOrEmpty(loginDto.EmailOrUsername) || string.IsNullOrEmpty(loginDto.Password))
        {
            return (false, null, "Email/username and password are required");
        }
        
        // Check for SQL injection
        if (_inputValidator.ContainsSqlInjection(loginDto.EmailOrUsername))
        {
            return (false, null, "Invalid input detected");
        }
        
        // Find user
        var user = await _userRepository.GetByEmailOrUsernameAsync(loginDto.EmailOrUsername);
        if (user == null)
        {
            // Use generic error message to prevent username enumeration
            return (false, null, "Invalid credentials");
        }
        
        // Verify password with constant-time comparison (handled by BCrypt)
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return (false, null, "Invalid credentials");
        }
        
        // Generate JWT token
        var token = GenerateJwtToken(user);
        
        return (true, token, "Login successful");
    }

    public async Task<(bool Success, string? Message)> RegisterAsync(UserRegistrationDto registrationDto)
    {
        // Validate email format
        if (!_inputValidator.ValidateEmail(registrationDto.Email))
        {
            return (false, "Invalid email format");
        }
        
        // Validate username format
        if (!_inputValidator.ValidateUsername(registrationDto.Username))
        {
            return (false, "Username must be 3-50 characters and can only contain letters, numbers, underscores and hyphens");
        }
        
        // Validate password strength
        if (!_inputValidator.ValidatePassword(registrationDto.Password))
        {
            return (false, "Password must be at least 12 characters and contain uppercase, lowercase, number, and special character");
        }
        
        // Check if passwords match
        if (registrationDto.Password != registrationDto.ConfirmPassword)
        {
            return (false, "Passwords do not match");
        }
        
        // Check if email exists
        var existingEmail = await _userRepository.GetByEmailAsync(registrationDto.Email);
        if (existingEmail != null)
        {
            return (false, "Email already exists");
        }
        
        // Check if username exists
        var existingUsername = await _userRepository.GetByUsernameAsync(registrationDto.Username);
        if (existingUsername != null)
        {
            return (false, "Username already exists");
        }
        
        // Create user with hashed password
        var user = new User
        {
            Email = registrationDto.Email.ToLower(),
            Username = registrationDto.Username,
            PasswordHash = _passwordHasher.HashPassword(registrationDto.Password),
            Role = "User", // Default role
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        var userId = await _userRepository.CreateAsync(user);
        
        return (userId > 0, "Registration successful");
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));
            
            // Validate token and signature
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);
            
            // Get user ID from token
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            
            // Verify user exists and is active
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null && user.IsActive;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        // Get user
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }
        
        // Verify current password
        if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return false;
        }
        
        // Validate new password strength
        if (!_inputValidator.ValidatePassword(changePasswordDto.NewPassword))
        {
            return false;
        }
        
        // Check if new passwords match
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
        {
            return false;
        }
        
        // Update password
        var newPasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
        return await _userRepository.ChangePasswordAsync(userId, newPasswordHash);
    }

    public async Task<bool> AssignRoleAsync(AssignRoleDto assignRoleDto, int currentUserId)
    {
        // Only Admin can assign roles
        var currentUser = await _userRepository.GetByIdAsync(currentUserId);
        if (currentUser == null || currentUser.Role != "Admin")
        {
            return false;
        }
        
        // Validate role
        if (assignRoleDto.Role != "Admin" && assignRoleDto.Role != "User")
        {
            return false;
        }
        
        // Update user role
        return await _userRepository.UpdateRoleAsync(assignRoleDto.UserId, assignRoleDto.Role);
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            
            if (jwtToken == null)
                return null;
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return null;
            
            return userId;
        }
        catch
        {
            return null;
        }
    }

    public string? GetRoleFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            
            if (jwtToken == null)
                return null;
            
            var roleClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            
            return roleClaim?.Value;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));
        
        // Set token expiry time
        var expiry = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(expiry),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"]
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
