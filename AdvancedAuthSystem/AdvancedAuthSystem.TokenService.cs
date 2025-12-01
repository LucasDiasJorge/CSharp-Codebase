using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AdvancedAuthSystem.Models;

namespace AdvancedAuthSystem.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, List<string> roles, List<UserClaim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    string GenerateTempToken(int userId);
    int? ValidateTempToken(string token);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? "AdvancedAuthSystem";
        _audience = _configuration["Jwt:Audience"] ?? "AdvancedAuthSystem";
    }

    public string GenerateAccessToken(User user, List<string> roles, List<UserClaim> userClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
            new("DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd"))
        };

        if (!string.IsNullOrEmpty(user.Department))
            claims.Add(new Claim("Department", user.Department));

        // Add roles
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Add custom claims
        foreach (var claim in userClaims)
            claims.Add(new Claim(claim.ClaimType, claim.ClaimValue));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string GenerateTempToken(int userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("TokenType", "TempToken")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int? ValidateTempToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
            return null;

        var tokenType = principal.FindFirst("TokenType")?.Value;
        if (tokenType != "TempToken")
            return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }
}
