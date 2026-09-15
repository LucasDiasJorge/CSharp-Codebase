namespace ApiKeyAuthenticationDemo.Keys;

/// <summary>
/// Chave de API como o servidor a guarda. O segredo em claro existe uma única vez, na
/// resposta da emissão; daqui em diante só há o hash.
/// </summary>
public sealed class ApiKey
{
    public ApiKey(string id, string secretHash, string owner, IReadOnlyList<string> scopes)
    {
        Id = id;
        SecretHash = secretHash;
        Owner = owner;
        Scopes = scopes;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Identificador público da chave, que viaja em claro dentro do próprio token. É por
    /// ele que a busca acontece — sem ele, validar exigiria comparar o segredo contra
    /// todas as chaves cadastradas.
    /// </summary>
    public string Id { get; }

    /// <summary>Hash do segredo. Comparado em tempo fixo, nunca com <c>==</c>.</summary>
    public string SecretHash { get; }

    public string Owner { get; }

    /// <summary>Permissões da chave. Viram claims e são checadas por policy.</summary>
    public IReadOnlyList<string> Scopes { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? LastUsedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Prazo de validade após a rotação. A chave antiga continua valendo por um tempo
    /// para que os clientes troquem sem downtime — rotação instantânea derruba todo
    /// mundo que ainda não atualizou.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>Chave que substituiu esta, quando houve rotação.</summary>
    public string? RotatedToKeyId { get; private set; }

    public bool IsUsable(DateTimeOffset now)
    {
        if (RevokedAt is not null)
        {
            return false;
        }

        return ExpiresAt is null || now < ExpiresAt;
    }

    public void MarkUsed(DateTimeOffset now)
    {
        LastUsedAt = now;
    }

    public void Revoke(DateTimeOffset now)
    {
        RevokedAt = now;
    }

    public void MarkRotated(string successorKeyId, DateTimeOffset expiresAt)
    {
        RotatedToKeyId = successorKeyId;
        ExpiresAt = expiresAt;
    }
}
