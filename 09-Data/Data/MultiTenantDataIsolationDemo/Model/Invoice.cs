namespace MultiTenantDataIsolationDemo.Model;

/// <summary>
/// Entidade pertencente a um tenant. Toda entidade multi-tenant precisa carregar essa
/// coluna — é ela que o filtro global usa, e é ela que precisa liderar os índices.
/// </summary>
public interface ITenantOwned
{
    string TenantId { get; set; }
}

public sealed class Invoice : ITenantOwned
{
    public int Id { get; set; }

    /// <summary>
    /// Dono do registro. O <c>private set</c> seria tentador, mas o filtro global e a
    /// atribuição automática precisam escrever aqui.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    public string Customer { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
