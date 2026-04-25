namespace BadOrderApi.Models;

// ❌ VIOLAÇÃO: Classes com muitas variáveis de instância (Regra 8)
// ❌ VIOLAÇÃO: Uso de primitivos sem encapsulamento (Regra 3)
// ❌ VIOLAÇÃO: Abreviações no código (Regra 6)
// ❌ VIOLAÇÃO: Getters e Setters expostos (Regra 9)
public class Order
{
    public int Id { get; set; }
    public string CustName { get; set; } = string.Empty; // ❌ Abreviação
    public string CustEmail { get; set; } = string.Empty; // ❌ Abreviação
    public string CustPhone { get; set; } = string.Empty; // ❌ Abreviação
    public string CustAddr { get; set; } = string.Empty; // ❌ Abreviação
    public string CustCity { get; set; } = string.Empty;
    public string CustZip { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new(); // ❌ Coleção exposta
    public decimal TotalAmt { get; set; } // ❌ Abreviação
    public string Status { get; set; } = "Pending"; // ❌ Primitivo para status
    public DateTime CreatedDt { get; set; } // ❌ Abreviação
    public DateTime? ShippedDt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public string DiscCode { get; set; } = string.Empty; // ❌ Abreviação
    public decimal DiscAmt { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProdName { get; set; } = string.Empty; // ❌ Abreviação
    public string ProdDesc { get; set; } = string.Empty; // ❌ Abreviação
    public int Qty { get; set; } // ❌ Abreviação
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Addr { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public List<Order> Orders { get; set; } = new(); // ❌ Coleção exposta
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public string Cat { get; set; } = string.Empty; // ❌ Abreviação (Category)
    public bool IsActive { get; set; }
}
