using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecurityAndAuthentication.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtKey;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _jwtKey = _configuration["Jwt:Key"] ?? "f7181daf86a42135a80611aee6b016fc1234567890abcdefghijklmnopqrstuvwxyz";
            _issuer = _configuration["Jwt:Issuer"] ?? "localhost:5150";
            _audience = _configuration["Jwt:Audience"] ?? "myfront.service.com";
        }

        public string GenerateJwtToken(string username, int userId, string role = "User")
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", userId.ToString()),
                new Claim("username", username),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Token válido por 2 horas
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token, out ClaimsPrincipal? principal)
        {
            principal = null;
            
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
                    ClockSkew = TimeSpan.Zero // Remove o delay padrão de 5 minutos
                };

                principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return false;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            if (ValidateToken(token, out ClaimsPrincipal? principal))
            {
                return principal?.FindFirst("userId")?.Value;
            }
            return null;
        }

        public string? GetUsernameFromToken(string token)
        {
            if (ValidateToken(token, out ClaimsPrincipal? principal))
            {
                return principal?.FindFirst("username")?.Value;
            }
            return null;
        }

        public string? GetRoleFromToken(string token)
        {
            if (ValidateToken(token, out ClaimsPrincipal? principal))
            {
                return principal?.FindFirst(ClaimTypes.Role)?.Value;
            }
            return null;
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true; // Se não conseguir ler o token, considera expirado
            }
        }
    }
}
