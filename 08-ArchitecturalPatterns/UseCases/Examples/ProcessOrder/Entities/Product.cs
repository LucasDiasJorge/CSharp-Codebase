using UseCases.Core;

namespace UseCases.Examples.ProcessOrder.Entities;

/// <summary>
/// Entidade de domínio: Produto
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }

    public Result ReserveStock(int quantity)
    {
        if (!IsActive)
            return Result.Failure("Produto indisponível");

        if (StockQuantity < quantity)
            return Result.Failure($"Estoque insuficiente para {Name}. Disponível: {StockQuantity}");

        StockQuantity -= quantity;
        return Result.Success();
    }
}
