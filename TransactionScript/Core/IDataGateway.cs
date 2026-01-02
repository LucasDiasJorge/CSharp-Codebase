namespace TransactionScript.Core;

/// <summary>
/// Interface do gateway de dados
/// Abstrai acesso ao banco de dados
/// </summary>
public interface IDataGateway
{
    // Accounts
    AccountData? GetAccount(Guid id);
    void SaveAccount(AccountData account);
    
    // Transactions
    void SaveTransaction(TransactionData transaction);
    
    // Invoices
    int GetNextInvoiceNumber();
    void SaveInvoice(InvoiceData invoice);
    
    // Products
    ProductData? GetProduct(Guid id);
    void SaveProduct(ProductData product);
}

// Data Transfer Objects simples
public class AccountData
{
    public Guid Id { get; set; }
    public string HolderName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
}

public class TransactionData
{
    public Guid Id { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class InvoiceData
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public List<InvoiceItemData> Items { get; set; } = [];
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class InvoiceItemData
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class ProductData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
