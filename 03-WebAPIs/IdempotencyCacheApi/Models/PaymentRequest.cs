using System.ComponentModel.DataAnnotations;

namespace IdempotencyCacheApi.Models;

public sealed class PaymentRequest
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    public string OrderId { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Amount { get; init; }

    [Required]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a 3 letter ISO code in uppercase.")]
    public string Currency { get; init; } = string.Empty;

    [StringLength(140)]
    public string? Description { get; init; }
}
