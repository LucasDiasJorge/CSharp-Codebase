namespace MultiTenantDataIsolationDemo.Data;

/// <summary>
/// Quem está pedindo. Em uma API isso viria do token ou do subdomínio, resolvido por
/// requisição; aqui é trocado à mão para que os cenários possam alternar de tenant.
/// </summary>
public sealed class TenantContext
{
    public TenantContext(string currentTenantId)
    {
        CurrentTenantId = currentTenantId;
    }

    public string CurrentTenantId { get; set; }
}
