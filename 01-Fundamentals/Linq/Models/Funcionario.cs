namespace LinqDemo;

/// <summary>
/// Representa um funcionário da empresa
/// </summary>
public class Funcionario
{
    /// <summary>
    /// Identificador único do funcionário
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome completo do funcionário
    /// </summary>
    public required string Nome { get; set; }
    
    /// <summary>
    /// Cargo do funcionário na empresa
    /// </summary>
    public required string Cargo { get; set; }
    
    /// <summary>
    /// Salário do funcionário
    /// </summary>
    public decimal Salario { get; set; }
    
    /// <summary>
    /// Identificador do gerente direto (null para CEO)
    /// </summary>
    public int? GerenteId { get; set; }
}
