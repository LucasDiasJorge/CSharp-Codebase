using Microsoft.EntityFrameworkCore;
using AdvancedAuthSystem.Data;
using AdvancedAuthSystem.Models;
using AdvancedAuthSystem.DTOs.Auth;

namespace AdvancedAuthSystem.Services;

public interface IAuthService
{
    Task<(bool Success, User? User, string? Error)> AuthenticateAsync(string username, string password);
    Task<(bool Success, User? User, string? Error)> RegisterAsync(RegisterRequest request);
    Task<RefreshToken?> CreateRefreshTokenAsync(int userId);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string reason);
    Task<(bool Success, string? Secret, string? QrCode, string? Error)> Enable2FAAsync(int userId);
    Task<bool> Verify2FAAsync(int userId, string code);
    Task<bool> Disable2FAAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;

    public AuthService(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ITwoFactorService twoFactorService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
    }

    public async Task<(bool Success, User? User, string? Error)> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserClaims)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return (false, null, "Invalid username or password");

        if (!user.IsActive)
            return (false, null, "Account is inactive");

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return (false, null, "Invalid username or password");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, user, null);
    }

    public async Task<(bool Success, User? User, string? Error)> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return (false, null, "Username already exists");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return (false, null, "Email already exists");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            DateOfBirth = request.DateOfBirth,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign default "User" role
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole != null)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id
            });

            await _context.SaveChangesAsync();
        }

        // Add department claim if provided
        if (!string.IsNullOrEmpty(request.Department))
        {
            _context.UserClaims.Add(new UserClaim
            {
                UserId = user.Id,
                ClaimType = "Department",
                ClaimValue = request.Department
            });

            await _context.SaveChangesAsync();
        }

        return (true, user, null);
    }

    public async Task<RefreshToken?> CreateRefreshTokenAsync(int userId)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserClaims)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            return null;

        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(string token, string reason)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedReason = reason;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(bool Success, string? Secret, string? QrCode, string? Error)> Enable2FAAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, null, null, "User not found");

        if (user.TwoFactorEnabled)
            return (false, null, null, "2FA already enabled");

        var secret = _twoFactorService.GenerateSecret();
        var qrCodeUrl = _twoFactorService.GenerateQrCodeUrl(user.Username, secret);

        user.TwoFactorSecret = secret;
        user.TwoFactorEnabled = true;
        await _context.SaveChangesAsync();

        return (true, secret, qrCodeUrl, null);
    }

    public async Task<bool> Verify2FAAsync(int userId, string code)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            return false;

        return _twoFactorService.ValidateCode(user.TwoFactorSecret, code);
    }

    public async Task<bool> Disable2FAAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        await _context.SaveChangesAsync();

        return true;
    }
}
