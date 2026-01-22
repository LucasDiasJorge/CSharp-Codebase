namespace LinqDemo;

/// <summary>
/// Representa um produto no sistema de e-commerce
/// </summary>
public class Produto
{
    /// <summary>
    /// Identificador único do produto
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do produto
    /// </summary>
    public required string Nome { get; set; }
    
    /// <summary>
    /// Categoria à qual o produto pertence
    /// </summary>
    public required string Categoria { get; set; }
    
    /// <summary>
    /// Preço unitário do produto
    /// </summary>
    public decimal Preco { get; set; }
    
    /// <summary>
    /// Indica se o produto está disponível em estoque
    /// </summary>
    public bool EmEstoque { get; set; }
    
    /// <summary>
    /// Quantidade disponível em estoque
    /// </summary>
    public int Estoque { get; set; }
}
