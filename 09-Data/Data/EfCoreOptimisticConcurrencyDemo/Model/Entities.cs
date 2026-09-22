namespace EfCoreOptimisticConcurrencyDemo.Model;

/// <summary>
/// Produto COM token de concorrência. O campo <see cref="Version"/> entra na cláusula
/// WHERE de todo UPDATE: se outro processo já gravou, o valor mudou, nenhuma linha é
/// afetada e o EF levanta <c>DbUpdateConcurrencyException</c>.
/// </summary>
public sealed class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    /// <summary>
    /// Token de concorrência. No SQL Server seria um `rowversion` mantido pelo banco;
    /// no SQLite não existe equivalente nativo, então o próprio contexto o incrementa
    /// a cada gravação — ver <c>ShopDbContext.SaveChangesAsync</c>.
    /// </summary>
    public int Version { get; set; }
}

/// <summary>
/// O mesmo produto SEM token, para comparação. Aqui o último UPDATE simplesmente vence
/// e o anterior desaparece — sem erro, sem aviso, sem registro.
/// </summary>
public sealed class UnguardedProduct
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }
}
