using RefreshTokenRotationDemo.Models;

namespace RefreshTokenRotationDemo.Tokens;

/// <summary>Desfechos possíveis de uma tentativa de rotação.</summary>
public enum RefreshStatus
{
    Success,

    /// <summary>Token desconhecido: nunca existiu ou o valor está errado.</summary>
    Unknown,

    /// <summary>Token válido, mas fora da validade dele ou da família.</summary>
    Expired,

    /// <summary>Família já derrubada por logout ou por reuso anterior.</summary>
    Revoked,

    /// <summary>
    /// Token já trocado sendo apresentado de novo. Existem duas cópias em circulação;
    /// a família inteira cai.
    /// </summary>
    ReuseDetected
}

public sealed class RefreshOutcome
{
    private RefreshOutcome(RefreshStatus status, TokenResponse? tokens, string? detail)
    {
        Status = status;
        Tokens = tokens;
        Detail = detail;
    }

    public RefreshStatus Status { get; }

    public TokenResponse? Tokens { get; }

    public string? Detail { get; }

    public static RefreshOutcome Success(TokenResponse tokens) => new RefreshOutcome(RefreshStatus.Success, tokens, null);

    public static RefreshOutcome Failure(RefreshStatus status, string detail) => new RefreshOutcome(status, null, detail);
}
