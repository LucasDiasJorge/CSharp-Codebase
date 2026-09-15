namespace PasskeyAuthenticationDemo.Credentials;

/// <summary>
/// O que o servidor guarda de uma passkey. Note o que **não** está aqui: nenhuma senha,
/// nenhum segredo compartilhado. Só a chave pública — a privada nunca sai do
/// autenticador. Um vazamento deste banco não permite entrar em conta nenhuma.
/// </summary>
public sealed class StoredCredential
{
    public StoredCredential(byte[] credentialId, byte[] publicKey, byte[] userHandle, uint signCounter, string username, Guid aaGuid)
    {
        CredentialId = credentialId;
        PublicKey = publicKey;
        UserHandle = userHandle;
        SignCounter = signCounter;
        Username = username;
        AaGuid = aaGuid;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Identificador da credencial, gerado pelo autenticador.</summary>
    public byte[] CredentialId { get; }

    /// <summary>Chave pública COSE. É com ela que a assinatura do login é verificada.</summary>
    public byte[] PublicKey { get; }

    /// <summary>
    /// Identificador opaco do usuário. Existe para não colocar e-mail ou nome dentro do
    /// autenticador — o que vaza de um dispositivo perdido é um identificador sem
    /// significado fora deste servidor.
    /// </summary>
    public byte[] UserHandle { get; }

    /// <summary>
    /// Contador de assinaturas. Se um login trouxer contador menor ou igual ao guardado,
    /// há indício de credencial clonada. Nem todo autenticador o implementa — muitos
    /// devolvem sempre zero, e aí a verificação não diz nada.
    /// </summary>
    public uint SignCounter { get; private set; }

    public string Username { get; }

    /// <summary>Modelo do autenticador. Útil para exibir "iPhone", "YubiKey" na lista.</summary>
    public Guid AaGuid { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? LastUsedAt { get; private set; }

    public void UpdateCounter(uint newCounter)
    {
        SignCounter = newCounter;
        LastUsedAt = DateTimeOffset.UtcNow;
    }
}
