namespace RefreshTokenRotationDemo.Tokens;

/// <summary>
/// Refresh token como o servidor o guarda. O valor em claro nunca é persistido — só o
/// hash. Quem obtiver uma cópia do banco não consegue usar nenhum token.
/// </summary>
public sealed class RefreshToken
{
    public RefreshToken(string tokenHash, string familyId, string userId, DateTimeOffset expiresAt, DateTimeOffset familyExpiresAt, string? parentTokenHash)
    {
        TokenHash = tokenHash;
        FamilyId = familyId;
        UserId = userId;
        ExpiresAt = expiresAt;
        FamilyExpiresAt = familyExpiresAt;
        ParentTokenHash = parentTokenHash;
        Status = RefreshTokenStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Hash do token. É por ele que a busca acontece — o claro não existe aqui.</summary>
    public string TokenHash { get; }

    /// <summary>
    /// Identifica a linhagem inteira: login original e todas as rotações que vieram
    /// dele. É o que permite revogar a sessão toda ao detectar reuso.
    /// </summary>
    public string FamilyId { get; }

    public string UserId { get; }

    public DateTimeOffset CreatedAt { get; }

    /// <summary>Validade deste token específico (janela deslizante).</summary>
    public DateTimeOffset ExpiresAt { get; }

    /// <summary>
    /// Teto absoluto da família. Sem ele, rotação indefinida vira sessão eterna: cada
    /// troca empurraria a validade para frente para sempre.
    /// </summary>
    public DateTimeOffset FamilyExpiresAt { get; }

    /// <summary>Token que originou este, para reconstruir a cadeia no diagnóstico.</summary>
    public string? ParentTokenHash { get; }

    public RefreshTokenStatus Status { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? RevokedReason { get; private set; }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt || now >= FamilyExpiresAt;

    public void MarkUsed(DateTimeOffset now)
    {
        Status = RefreshTokenStatus.Used;
        UsedAt = now;
    }

    public void Revoke(DateTimeOffset now, string reason)
    {
        Status = RefreshTokenStatus.Revoked;
        RevokedAt = now;
        RevokedReason = reason;
    }
}
