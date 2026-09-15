using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefreshTokenRotationDemo.Models;
using RefreshTokenRotationDemo.Tokens;
using RefreshTokenRotationDemo.Users;

namespace RefreshTokenRotationDemo.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly UserStore _users;
    private readonly RefreshTokenStore _store;

    public AuthController(TokenService tokenService, UserStore users, RefreshTokenStore store)
    {
        _tokenService = tokenService;
        _users = users;
        _store = store;
    }

    /// <summary>Abre uma família de tokens nova.</summary>
    [HttpPost("login")]
    public ActionResult<TokenResponse> Login(LoginRequest request)
    {
        DemoUser? user = _users.Validate(request.Username!, request.Password!);
        if (user is null)
        {
            return Unauthorized(new { error = "Credenciais invalidas." });
        }

        return Ok(_tokenService.IssueForLogin(user));
    }

    /// <summary>
    /// Troca o refresh token por um par novo. O token apresentado deixa de valer no
    /// mesmo instante — daí "rotação".
    /// </summary>
    [HttpPost("refresh")]
    public ActionResult<TokenResponse> Refresh(RefreshRequest request)
    {
        RefreshOutcome outcome = _tokenService.Rotate(request.RefreshToken!);

        if (outcome.Status == RefreshStatus.Success)
        {
            return Ok(outcome.Tokens);
        }

        // Reuso responde 401 como qualquer outra falha: o cliente não precisa saber que
        // a detecção existe, e o atacante muito menos.
        return Unauthorized(new
        {
            error = outcome.Status.ToString(),
            detail = outcome.Detail
        });
    }

    /// <summary>Logout: revoga a família inteira, não só o token enviado.</summary>
    [HttpPost("logout")]
    public ActionResult Logout(RefreshRequest request)
    {
        int revoked = _tokenService.RevokeFamilyOf(request.RefreshToken!);

        return Ok(new { revokedTokens = revoked });
    }

    /// <summary>Endpoint protegido: só serve para comprovar que o access token vale.</summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult Me()
    {
        List<object> claims = new List<object>();
        foreach (Claim claim in User.Claims)
        {
            claims.Add(new { type = claim.Type, value = claim.Value });
        }

        return Ok(new
        {
            user = User.Identity?.Name,
            claims
        });
    }

    /// <summary>
    /// Inspeção do estado dos tokens. Não existiria em produção — está aqui para tornar
    /// rotação, reuso e revogação visíveis sem abrir o banco.
    /// </summary>
    [HttpGet("tokens")]
    public ActionResult Tokens()
    {
        List<object> tokens = new List<object>();
        foreach (RefreshToken token in _store.GetAll())
        {
            tokens.Add(new
            {
                // Prefixo do hash apenas: o suficiente para acompanhar a cadeia na tela.
                hash = token.TokenHash[..12],
                parent = token.ParentTokenHash is null ? null : token.ParentTokenHash[..12],
                family = token.FamilyId[..8],
                status = token.Status.ToString(),
                createdAt = token.CreatedAt,
                usedAt = token.UsedAt,
                revokedAt = token.RevokedAt,
                revokedReason = token.RevokedReason
            });
        }

        return Ok(tokens);
    }
}
