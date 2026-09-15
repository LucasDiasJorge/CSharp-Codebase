using System.Security.Claims;
using System.Text.Encodings.Web;
using ApiKeyAuthenticationDemo.Keys;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ApiKeyAuthenticationDemo.Authentication;

/// <summary>
/// Esquema de autenticação por chave de API. Um handler é o ponto em que uma credencial
/// qualquer vira um <see cref="ClaimsPrincipal"/> — daí para a frente, todo o resto do
/// ASP.NET Core (autorização por policy inclusive) funciona sem saber de onde ela veio.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string HeaderName = "X-Api-Key";
    public const string ScopeClaim = "scope";
    public const string KeyIdClaim = "key_id";

    private readonly ApiKeyStore _keys;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ApiKeyStore keys)
        : base(options, logger, encoder)
    {
        _keys = keys;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out Microsoft.Extensions.Primitives.StringValues header))
        {
            // NoResult, e não Fail: ninguém apresentou credencial deste tipo. A distinção
            // importa quando há mais de um esquema registrado — Fail encerra a cadeia,
            // NoResult deixa outro esquema tentar.
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string presented = header.ToString();

        ApiKey? key = _keys.Validate(presented, out string failureReason);

        if (key is null)
        {
            // Aqui sim: veio credencial e ela não serve.
            Logger.LogWarning("Autenticacao por chave recusada: {Motivo}", failureReason);

            return Task.FromResult(AuthenticateResult.Fail(failureReason));
        }

        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, key.Id),
            new Claim(ClaimTypes.Name, key.Owner),
            new Claim(KeyIdClaim, key.Id)
        ];

        // Cada escopo vira uma claim. É o que permite autorizar por policy em vez de
        // espalhar verificação de permissão pelos endpoints.
        foreach (string scope in key.Scopes)
        {
            claims.Add(new Claim(ScopeClaim, scope));
        }

        ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    /// <summary>
    /// 401 com o header que diz ao cliente como se autenticar. Sem isso, o cliente recebe
    /// um 401 mudo e não sabe o que faltou.
    /// </summary>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = $"ApiKey realm=\"{ApiKeyDefaults.Realm}\", header=\"{HeaderName}\"";

        return Task.CompletedTask;
    }
}

public static class ApiKeyDefaults
{
    public const string AuthenticationScheme = "ApiKey";
    public const string Realm = "ApiKeyAuthenticationDemo";
}
