namespace LinqDemo;

/// <summary>
/// Representa um cliente do sistema
/// </summary>
public class Cliente
{
    /// <summary>
    /// Identificador único do cliente
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome completo do cliente
    /// </summary>
    public required string Nome { get; set; }
    
    /// <summary>
    /// Endereço de e-mail do cliente
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Cidade onde o cliente reside
    /// </summary>
    public required string Cidade { get; set; }
    
    /// <summary>
    /// Indica se o cliente possui status Premium
    /// </summary>
    public bool Premium { get; set; }
}
