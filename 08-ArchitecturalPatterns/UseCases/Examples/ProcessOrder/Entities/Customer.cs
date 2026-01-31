namespace UseCases.Examples.ProcessOrder.Entities;

/// <summary>
/// Entidade de domínio: Cliente
/// </summary>
public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public CustomerTier Tier { get; set; }
    public int TotalOrders { get; set; }

    public decimal GetDiscountPercentage()
    {
        return Tier switch
        {
            CustomerTier.Bronze => 0,
            CustomerTier.Silver => 5,
            CustomerTier.Gold => 10,
            CustomerTier.Platinum => 15,
            _ => 0
        };
    }
}
