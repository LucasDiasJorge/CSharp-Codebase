using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace ApiKeyAuthenticationDemo.Keys;

/// <summary>
/// Emissão, validação, rotação e revogação das chaves. O formato do token é o que
/// permite que tudo isso seja barato: prefixo, id e segredo, separados por underscore.
/// </summary>
public sealed class ApiKeyStore
{
    /// <summary>
    /// Prefixo fixo. Serve para duas coisas práticas: identificar o tipo de credencial ao
    /// olhar um log ou um commit, e permitir que ferramentas de secret scanning
    /// reconheçam a chave por padrão e avisem quando ela vazar para um repositório.
    /// </summary>
    public const string Prefix = "cbk";

    private static readonly TimeSpan RotationGracePeriod = TimeSpan.FromMinutes(10);

    private readonly ConcurrentDictionary<string, ApiKey> _keysById = new ConcurrentDictionary<string, ApiKey>();
    private readonly ILogger<ApiKeyStore> _logger;

    public ApiKeyStore(ILogger<ApiKeyStore> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Emite uma chave. O valor em claro volta aqui e nunca mais: não há como reexibi-lo,
    /// porque o servidor não o tem.
    /// </summary>
    public ApiKeyIssueResult Issue(string owner, IReadOnlyList<string> scopes)
    {
        string id = Base64UrlEncode(RandomNumberGenerator.GetBytes(8));
        string secret = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        ApiKey key = new ApiKey(id, HashSecret(secret), owner, scopes);
        _keysById[id] = key;

        _logger.LogInformation("Chave {ChaveId} emitida para {Dono} com escopos {Escopos}.", id, owner, string.Join(",", scopes));

        return new ApiKeyIssueResult(key, Prefix + "_" + id + "_" + secret);
    }

    /// <summary>
    /// Emite a sucessora e dá à antiga um prazo de graça. Rotação sem sobreposição
    /// derruba todo cliente que ainda não atualizou a configuração.
    /// </summary>
    public ApiKeyIssueResult? Rotate(string keyId)
    {
        if (!_keysById.TryGetValue(keyId, out ApiKey? current) || current.RevokedAt is not null)
        {
            return null;
        }

        ApiKeyIssueResult successor = Issue(current.Owner, current.Scopes);
        current.MarkRotated(successor.Key.Id, DateTimeOffset.UtcNow.Add(RotationGracePeriod));

        _logger.LogInformation(
            "Chave {Antiga} rotacionada para {Nova}; a antiga expira em {Minutos} minutos.",
            keyId,
            successor.Key.Id,
            RotationGracePeriod.TotalMinutes);

        return successor;
    }

    public bool Revoke(string keyId)
    {
        if (!_keysById.TryGetValue(keyId, out ApiKey? key))
        {
            return false;
        }

        key.Revoke(DateTimeOffset.UtcNow);
        _logger.LogWarning("Chave {ChaveId} revogada.", keyId);

        return true;
    }

    /// <summary>
    /// Valida o token apresentado. Duas etapas com propriedades diferentes: a busca é por
    /// id (barata, indexada) e a conferência do segredo é em tempo fixo.
    /// </summary>
    public ApiKey? Validate(string presentedToken, out string failureReason)
    {
        string[] parts = presentedToken.Split('_');

        if (parts.Length != 3 || parts[0] != Prefix)
        {
            failureReason = "Formato de chave invalido.";

            return null;
        }

        if (!_keysById.TryGetValue(parts[1], out ApiKey? key))
        {
            failureReason = "Chave desconhecida.";

            return null;
        }

        // CryptographicOperations.FixedTimeEquals em vez de == : comparação de string
        // retorna no primeiro byte diferente, e essa diferença de tempo, medida em muitas
        // tentativas, revelaria o segredo byte a byte. O custo aqui é nenhum.
        byte[] expected = Encoding.UTF8.GetBytes(key.SecretHash);
        byte[] presented = Encoding.UTF8.GetBytes(HashSecret(parts[2]));

        if (!CryptographicOperations.FixedTimeEquals(expected, presented))
        {
            failureReason = "Segredo invalido.";

            return null;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (!key.IsUsable(now))
        {
            failureReason = key.RevokedAt is not null
                ? "Chave revogada."
                : "Chave expirada apos rotacao.";

            return null;
        }

        key.MarkUsed(now);
        failureReason = string.Empty;

        return key;
    }

    public IReadOnlyList<ApiKey> GetAll()
    {
        List<ApiKey> keys = new List<ApiKey>(_keysById.Values);
        keys.Sort((left, right) => left.CreatedAt.CompareTo(right.CreatedAt));

        return keys;
    }

    /// <summary>
    /// SHA-256 sem KDF, pela mesma razão do refresh token: o segredo tem 256 bits de
    /// entropia aleatória e não é alvo viável de força bruta. Senha exigiria Argon2.
    /// </summary>
    private static string HashSecret(string secret)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}

public sealed class ApiKeyIssueResult
{
    public ApiKeyIssueResult(ApiKey key, string plaintextToken)
    {
        Key = key;
        PlaintextToken = plaintextToken;
    }

    public ApiKey Key { get; }

    /// <summary>Visível uma única vez, na resposta da emissão.</summary>
    public string PlaintextToken { get; }
}
