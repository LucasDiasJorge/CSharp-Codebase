using CustomFilterApi.Attributes;

namespace CustomFilterApi.Models;

/// <summary>
/// Modelo representando um produto.
/// Demonstra outro cenário de uso do atributo [LogProperty].
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Nome do produto será logado
    /// </summary>
    [LogProperty]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Preço será logado
    /// </summary>
    [LogProperty]
    public decimal Price { get; set; }

    /// <summary>
    /// Categoria será logada com nome customizado
    /// </summary>
    [LogProperty(logName: "Categoria do Produto")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Descrição NÃO será logada (sem atributo)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Estoque NÃO será logado (sem atributo)
    /// </summary>
    public int Stock { get; set; }
}
