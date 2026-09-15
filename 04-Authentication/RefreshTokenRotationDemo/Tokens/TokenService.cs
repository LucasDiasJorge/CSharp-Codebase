using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RefreshTokenRotationDemo.Models;
using RefreshTokenRotationDemo.Users;

namespace RefreshTokenRotationDemo.Tokens;

/// <summary>
/// Emissão e rotação de tokens. As três políticas do exemplo vivem aqui: access token
/// curto, refresh token de uso único e detecção de reuso por família.
/// </summary>
public sealed class TokenService
{
    /// <summary>
    /// 60 segundos é curto de propósito — em produção seria algo entre 5 e 15 minutos.
    /// O access token curto existe para limitar a janela de dano se ele vazar: sendo
    /// auto-contido, não há como revogá-lo antes de expirar.
    /// </summary>
    public const int AccessTokenLifetimeSeconds = 60;

    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan FamilyLifetime = TimeSpan.FromDays(30);

    private readonly RefreshTokenStore _store;
    private readonly UserStore _users;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<TokenService> _logger;

    public TokenService(RefreshTokenStore store, UserStore users, JwtOptions jwtOptions, ILogger<TokenService> logger)
    {
        _store = store;
        _users = users;
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    /// <summary>Login: abre uma família nova.</summary>
    public TokenResponse IssueForLogin(DemoUser user)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string familyId = Guid.NewGuid().ToString("N");

        _logger.LogInformation("Login de {Usuario}: familia {Familia} aberta.", user.Username, familyId);

        return IssuePair(user, familyId, now, now.Add(FamilyLifetime), null);
    }

    /// <summary>
    /// Rotação: valida, invalida o token apresentado e emite um par novo na mesma
    /// família. Um refresh token vale exatamente uma troca.
    /// </summary>
    public RefreshOutcome Rotate(string presentedToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        RefreshToken? stored = _store.FindByToken(presentedToken);

        if (stored is null)
        {
            return RefreshOutcome.Failure(RefreshStatus.Unknown, "Refresh token desconhecido.");
        }

        if (stored.Status == RefreshTokenStatus.Used)
        {
            // Reuso: este token já foi trocado. Ou o cliente legítimo repetiu a
            // requisição, ou alguém copiou o token — e não há como distinguir os dois.
            // A saída segura é derrubar a linhagem inteira e exigir novo login.
            int revoked = _store.RevokeFamily(stored.FamilyId, now, "reuso de refresh token detectado");

            _logger.LogWarning(
                "Reuso detectado na familia {Familia} do usuario {UsuarioId}. {Quantidade} tokens revogados.",
                stored.FamilyId,
                stored.UserId,
                revoked);

            return RefreshOutcome.Failure(
                RefreshStatus.ReuseDetected,
                "Refresh token ja utilizado. A familia " + stored.FamilyId + " foi revogada (" + revoked + " tokens).");
        }

        if (stored.Status == RefreshTokenStatus.Revoked)
        {
            return RefreshOutcome.Failure(RefreshStatus.Revoked, "Refresh token revogado: " + stored.RevokedReason + ".");
        }

        if (stored.IsExpired(now))
        {
            return RefreshOutcome.Failure(RefreshStatus.Expired, "Refresh token expirado.");
        }

        DemoUser? user = _users.FindById(stored.UserId);
        if (user is null)
        {
            return RefreshOutcome.Failure(RefreshStatus.Unknown, "Usuario do token nao existe mais.");
        }

        // Marcar como usado ANTES de emitir o próximo: é esta marca que transforma uma
        // reapresentação futura em sinal de vazamento.
        stored.MarkUsed(now);

        TokenResponse tokens = IssuePair(user, stored.FamilyId, now, stored.FamilyExpiresAt, stored.TokenHash);

        _logger.LogInformation("Rotacao concluida na familia {Familia}.", stored.FamilyId);

        return RefreshOutcome.Success(tokens);
    }

    /// <summary>Logout: derruba a família, não apenas o token atual.</summary>
    public int RevokeFamilyOf(string presentedToken)
    {
        RefreshToken? stored = _store.FindByToken(presentedToken);
        if (stored is null)
        {
            return 0;
        }

        return _store.RevokeFamily(stored.FamilyId, DateTimeOffset.UtcNow, "logout");
    }

    private TokenResponse IssuePair(DemoUser user, string familyId, DateTimeOffset now, DateTimeOffset familyExpiresAt, string? parentHash)
    {
        string accessToken = CreateAccessToken(user, now);
        string refreshToken = RefreshTokenStore.GenerateToken();
        DateTimeOffset refreshExpiresAt = now.Add(RefreshTokenLifetime);

        if (refreshExpiresAt > familyExpiresAt)
        {
            // O teto absoluto da família vence a janela deslizante: sem ele, rotação
            // indefinida viraria sessão eterna.
            refreshExpiresAt = familyExpiresAt;
        }

        _store.Add(new RefreshToken(
            RefreshTokenStore.HashToken(refreshToken),
            familyId,
            user.Id,
            refreshExpiresAt,
            familyExpiresAt,
            parentHash));

        return new TokenResponse(accessToken, AccessTokenLifetimeSeconds, refreshToken, refreshExpiresAt, familyId);
    }

    private string CreateAccessToken(DemoUser user, DateTimeOffset now)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),

            // jti unico por token: serve para auditoria e para uma eventual denylist.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        ];

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddSeconds(AccessTokenLifetimeSeconds).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>Parâmetros de assinatura e validação do access token.</summary>
public sealed class JwtOptions
{
    public JwtOptions(string issuer, string audience, string signingKey)
    {
        Issuer = issuer;
        Audience = audience;
        SigningKey = signingKey;
    }

    public string Issuer { get; }

    public string Audience { get; }

    public string SigningKey { get; }
}
