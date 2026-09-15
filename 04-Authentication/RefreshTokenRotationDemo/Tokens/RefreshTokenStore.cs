using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace RefreshTokenRotationDemo.Tokens;

/// <summary>
/// Armazenamento dos refresh tokens, indexado pelo hash. Em memória por ser um exemplo;
/// o que importa aqui é a política, não o meio de persistência.
/// </summary>
public sealed class RefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshToken> _tokensByHash = new ConcurrentDictionary<string, RefreshToken>();

    /// <summary>
    /// Gera um token de 256 bits de entropia. O valor em claro volta para o cliente uma
    /// única vez; o servidor guarda apenas o hash.
    /// </summary>
    public static string GenerateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);

        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// SHA-256 puro, sem salt e sem KDF — e isso é correto **aqui**. BCrypt e afins
    /// existem para segredos de baixa entropia (senhas), onde força bruta é viável.
    /// Um token aleatório de 256 bits não é adivinhável, então o custo de um KDF em toda
    /// requisição de refresh compraria nada. Para senha, a resposta é outra.
    /// </summary>
    public static string HashToken(string token)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hash);
    }

    public void Add(RefreshToken token)
    {
        _tokensByHash[token.TokenHash] = token;
    }

    /// <summary>
    /// Busca pelo hash. Como é lookup por chave, não há comparação byte a byte do
    /// segredo e, portanto, não há canal lateral de tempo a proteger.
    /// </summary>
    public RefreshToken? FindByToken(string token)
    {
        return _tokensByHash.TryGetValue(HashToken(token), out RefreshToken? found) ? found : null;
    }

    public IReadOnlyList<RefreshToken> GetFamily(string familyId)
    {
        List<RefreshToken> family = new List<RefreshToken>();
        foreach (RefreshToken token in _tokensByHash.Values)
        {
            if (token.FamilyId == familyId)
            {
                family.Add(token);
            }
        }

        family.Sort((left, right) => left.CreatedAt.CompareTo(right.CreatedAt));

        return family;
    }

    /// <summary>
    /// Revoga a linhagem inteira. É a resposta à detecção de reuso: não dá para saber se
    /// quem apresentou o token repetido foi o dono ou o atacante, então derruba-se a
    /// sessão dos dois.
    /// </summary>
    public int RevokeFamily(string familyId, DateTimeOffset now, string reason)
    {
        int revoked = 0;
        foreach (RefreshToken token in _tokensByHash.Values)
        {
            if (token.FamilyId == familyId && token.Status != RefreshTokenStatus.Revoked)
            {
                token.Revoke(now, reason);
                revoked++;
            }
        }

        return revoked;
    }

    public IReadOnlyList<RefreshToken> GetAll()
    {
        List<RefreshToken> all = new List<RefreshToken>(_tokensByHash.Values);
        all.Sort((left, right) => left.CreatedAt.CompareTo(right.CreatedAt));

        return all;
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
