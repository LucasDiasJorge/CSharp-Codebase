using System.Collections.Concurrent;
using System.Security.Cryptography;
using Fido2NetLib;

namespace PasskeyAuthenticationDemo.Credentials;

/// <summary>
/// Credenciais registradas e desafios pendentes. Em memória por ser exemplo; o que
/// importa é a política de uso dos desafios, não o meio de persistência.
/// </summary>
public sealed class CredentialStore
{
    private readonly ConcurrentDictionary<string, StoredCredential> _credentialsById = new ConcurrentDictionary<string, StoredCredential>();
    private readonly ConcurrentDictionary<string, byte[]> _userHandlesByName = new ConcurrentDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, PendingCeremony> _pendingCeremonies = new ConcurrentDictionary<string, PendingCeremony>();

    /// <summary>
    /// Handle estável por usuário. Gerado uma vez e reutilizado: mudar o handle faria o
    /// autenticador tratar o mesmo usuário como outra pessoa e criar credencial duplicada.
    /// </summary>
    public byte[] GetOrCreateUserHandle(string username)
    {
        return _userHandlesByName.GetOrAdd(username, _ => RandomNumberGenerator.GetBytes(32));
    }

    /// <summary>
    /// Guarda as opções emitidas (com o desafio) para conferir na conclusão. O desafio
    /// precisa ser verificado contra o que o servidor mandou — aceitar o que o cliente
    /// devolve anula a proteção contra replay.
    /// </summary>
    public string StoreRegistrationCeremony(CredentialCreateOptions options, string username)
    {
        string ceremonyId = Guid.NewGuid().ToString("N");
        _pendingCeremonies[ceremonyId] = new PendingCeremony(options.ToJson(), null, username);

        return ceremonyId;
    }

    public string StoreAssertionCeremony(AssertionOptions options, string? username)
    {
        string ceremonyId = Guid.NewGuid().ToString("N");
        _pendingCeremonies[ceremonyId] = new PendingCeremony(null, options.ToJson(), username);

        return ceremonyId;
    }

    /// <summary>
    /// Consome a cerimônia: remove ao ler. Um desafio vale uma única vez — deixá-lo
    /// disponível para uma segunda conclusão abriria espaço para replay.
    /// </summary>
    public PendingCeremony? TakeCeremony(string ceremonyId)
    {
        return _pendingCeremonies.TryRemove(ceremonyId, out PendingCeremony? ceremony) ? ceremony : null;
    }

    public void Add(StoredCredential credential)
    {
        _credentialsById[Convert.ToBase64String(credential.CredentialId)] = credential;
    }

    public StoredCredential? FindByCredentialId(byte[] credentialId)
    {
        return _credentialsById.TryGetValue(Convert.ToBase64String(credentialId), out StoredCredential? credential)
            ? credential
            : null;
    }

    public IReadOnlyList<StoredCredential> FindByUsername(string username)
    {
        List<StoredCredential> found = new List<StoredCredential>();
        foreach (StoredCredential credential in _credentialsById.Values)
        {
            if (string.Equals(credential.Username, username, StringComparison.OrdinalIgnoreCase))
            {
                found.Add(credential);
            }
        }

        return found;
    }

    public IReadOnlyList<StoredCredential> FindByUserHandle(byte[] userHandle)
    {
        List<StoredCredential> found = new List<StoredCredential>();
        foreach (StoredCredential credential in _credentialsById.Values)
        {
            if (credential.UserHandle.AsSpan().SequenceEqual(userHandle))
            {
                found.Add(credential);
            }
        }

        return found;
    }

    public IReadOnlyList<StoredCredential> GetAll()
    {
        return new List<StoredCredential>(_credentialsById.Values);
    }

    public bool IsCredentialIdUnique(byte[] credentialId)
    {
        return !_credentialsById.ContainsKey(Convert.ToBase64String(credentialId));
    }
}

/// <summary>Cerimônia em andamento: as opções emitidas, aguardando conclusão.</summary>
public sealed class PendingCeremony
{
    public PendingCeremony(string? creationOptionsJson, string? assertionOptionsJson, string? username)
    {
        CreationOptionsJson = creationOptionsJson;
        AssertionOptionsJson = assertionOptionsJson;
        Username = username;
    }

    public string? CreationOptionsJson { get; }

    public string? AssertionOptionsJson { get; }

    public string? Username { get; }
}
