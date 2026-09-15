using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PolicyBasedAuthorizationDemo.Authorization.Handlers;

namespace PolicyBasedAuthorizationDemo.Users;

/// <summary>
/// Emite tokens para os perfis fixos do exemplo. Autenticação aqui é andaime: o assunto
/// do projeto é o que acontece **depois** que o usuário já foi identificado. Emissão de
/// token com rotação e revogação é o tema de
/// <c>RefreshTokenRotationDemo</c>.
/// </summary>
public sealed class DemoTokenIssuer
{
    public const string Issuer = "PolicyBasedAuthorizationDemo";
    public const string Audience = "PolicyBasedAuthorizationDemo.Clients";
    public const string SigningKey = "chave-de-desenvolvimento-com-pelo-menos-32-bytes!!";
    public const string DepartmentClaim = "department";

    private static readonly IReadOnlyDictionary<string, DemoProfile> Profiles = new Dictionary<string, DemoProfile>(StringComparer.OrdinalIgnoreCase)
    {
        // clearance alto, engenharia, admin: passa em tudo
        ["ana"] = new DemoProfile("1", "ana", "engineering", clearance: 5, role: "admin"),

        // engenharia, clearance media: dono do documento 2
        ["bruno"] = new DemoProfile("2", "bruno", "engineering", clearance: 3, role: "member"),

        // financeiro, clearance baixa: reprovado na maior parte das policies
        ["carla"] = new DemoProfile("3", "carla", "finance", clearance: 1, role: "member")
    };

    public IReadOnlyCollection<string> KnownUsers => (IReadOnlyCollection<string>)Profiles.Keys;

    public string? IssueFor(string username)
    {
        if (!Profiles.TryGetValue(username, out DemoProfile? profile))
        {
            return null;
        }

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, profile.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, profile.Username),
            new Claim(ClaimTypes.Role, profile.Role),
            new Claim(DepartmentClaim, profile.Department),
            new Claim(MinimumClearanceHandler.ClearanceClaim, profile.Clearance.ToString())
        ];

        SigningCredentials credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed class DemoProfile
{
    public DemoProfile(string id, string username, string department, int clearance, string role)
    {
        Id = id;
        Username = username;
        Department = department;
        Clearance = clearance;
        Role = role;
    }

    public string Id { get; }

    public string Username { get; }

    public string Department { get; }

    public int Clearance { get; }

    public string Role { get; }
}
