using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoUserApi.Models;

namespace MongoUserApi.Services;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResponse GenerateToken(User user)
    {
        string? jwtKey = _configuration["Jwt:Key"];
        string? jwtIssuer = _configuration["Jwt:Issuer"];
        string? jwtAudience = _configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("Jwt:Key missing");
        }
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtKey);
        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(keyBytes);
        SigningCredentials credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role),
            new Claim("active", user.Active ? "true" : "false"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        DateTime expires = DateTime.UtcNow.AddHours(2);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        string encoded = new JwtSecurityTokenHandler().WriteToken(token);
        AuthResponse response = new AuthResponse
        {
            Token = encoded,
            ExpiresAt = expires
        };
        return response;
    }
}
