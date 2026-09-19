namespace ProxyRemoteServiceDemo.Contracts;

/// <summary>
/// Quem está chamando. O proxy de autorização precisa disto; o serviço real, não —
/// e é essa separação que mantém a regra de acesso fora do domínio.
/// </summary>
public sealed class CallerContext
{
    public CallerContext(string user, IReadOnlyCollection<string> permissions)
    {
        User = user;
        Permissions = permissions;
    }

    public string User { get; }

    public IReadOnlyCollection<string> Permissions { get; }

    public bool Has(string permission) => Permissions.Contains(permission);
}
