namespace TransactionScript.Core;

/// <summary>
/// Implementação em memória do Data Gateway
/// </summary>
public class InMemoryDataGateway : IDataGateway
{
    private readonly Dictionary<Guid, AccountData> _accounts = [];
    private readonly Dictionary<Guid, ProductData> _products = [];
    private readonly List<TransactionData> _transactions = [];
    private readonly List<InvoiceData> _invoices = [];
    private int _invoiceCounter = 1000;

    public InMemoryDataGateway()
    {
        // Seed data
        SeedData();
    }

    private void SeedData()
    {
        // Contas
        var account1 = new AccountData
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            HolderName = "João Silva",
            Balance = 5000.00m,
            IsActive = true
        };

        var account2 = new AccountData
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            HolderName = "Maria Santos",
            Balance = 3000.00m,
            IsActive = true
        };

        _accounts[account1.Id] = account1;
        _accounts[account2.Id] = account2;

        // Produtos
        var product1 = new ProductData
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Name = "Notebook",
            Price = 3500.00m,
            Stock = 10
        };

        _products[product1.Id] = product1;
    }

    public AccountData? GetAccount(Guid id)
    {
        _accounts.TryGetValue(id, out var account);
        Console.WriteLine($"    [DB] GetAccount({id}) -> {account?.HolderName ?? "não encontrado"}");
        return account;
    }

    public void SaveAccount(AccountData account)
    {
        _accounts[account.Id] = account;
        Console.WriteLine($"    [DB] SaveAccount({account.HolderName}) -> Saldo: {account.Balance:C}");
    }

    public void SaveTransaction(TransactionData transaction)
    {
        _transactions.Add(transaction);
        Console.WriteLine($"    [DB] SaveTransaction({transaction.Amount:C})");
    }

    public int GetNextInvoiceNumber()
    {
        return ++_invoiceCounter;
    }

    public void SaveInvoice(InvoiceData invoice)
    {
        _invoices.Add(invoice);
        Console.WriteLine($"    [DB] SaveInvoice(#{invoice.Number}) -> {invoice.Total:C}");
    }

    public ProductData? GetProduct(Guid id)
    {
        _products.TryGetValue(id, out var product);
        Console.WriteLine($"    [DB] GetProduct({id}) -> {product?.Name ?? "não encontrado"}");
        return product;
    }

    public void SaveProduct(ProductData product)
    {
        _products[product.Id] = product;
        Console.WriteLine($"    [DB] SaveProduct({product.Name}) -> Stock: {product.Stock}");
    }
}
