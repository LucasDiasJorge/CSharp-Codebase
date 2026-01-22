namespace LinqDemo;

/// <summary>
/// Representa um pedido realizado por um cliente
/// </summary>
public class Pedido
{
    /// <summary>
    /// Identificador único do pedido
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Identificador do cliente que realizou o pedido
    /// </summary>
    public int ClienteId { get; set; }
    
    /// <summary>
    /// Identificador do produto pedido
    /// </summary>
    public int ProdutoId { get; set; }
    
    /// <summary>
    /// Quantidade de produtos no pedido
    /// </summary>
    public int Quantidade { get; set; }
    
    /// <summary>
    /// Data em que o pedido foi realizado
    /// </summary>
    public DateTime DataPedido { get; set; }
}
