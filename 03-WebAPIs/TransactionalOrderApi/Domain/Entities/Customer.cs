namespace TransactionalOrderApi.Domain.Entities;

public class Customer
{
    private Customer() { }

    public Customer(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
    }

    public int Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public decimal TotalSpent { get; private set; }
    public ICollection<Order> Orders { get; } = new List<Order>();

    public void RegisterPurchase(decimal amount) => TotalSpent += amount;
}
