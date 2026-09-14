using System.ComponentModel.DataAnnotations;

namespace ProblemDetailsApi.Models;

/// <summary>
/// Entrada da criação de pedido. As anotações cobrem apenas formato; regra de negócio
/// (estoque, estado do pedido) fica no serviço e produz 422, não 400.
/// </summary>
public sealed class CreateOrderRequest
{
    [Required(ErrorMessage = "O SKU e obrigatorio.")]
    [RegularExpression("^[A-Z]{3}-[0-9]{4}$", ErrorMessage = "O SKU deve seguir o formato AAA-0000.")]
    public string? Sku { get; set; }

    [Range(1, 100, ErrorMessage = "A quantidade deve estar entre 1 e 100.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "O cliente e obrigatorio.")]
    [EmailAddress(ErrorMessage = "O cliente deve ser um e-mail valido.")]
    public string? CustomerEmail { get; set; }
}
