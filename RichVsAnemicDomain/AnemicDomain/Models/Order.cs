namespace AnemicDomain.Models;

/// <summary>
/// Exemplo de Modelo Anêmico: Apenas dados, sem comportamento
/// Problema: Esta classe é apenas uma "sacola de dados"
/// </summary>
public class Order
{
    // ❌ Tudo público - sem proteção
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Discount { get; set; }
    
    // ❌ Sem validação, sem comportamento
    // Qualquer código pode modificar qualquer propriedade
    // Não há garantia de que o objeto estará em um estado válido
}

/// <summary>
/// Item de pedido anêmico
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    
    // ❌ Sem validação de quantidade mínima
    // ❌ Sem validação de preços negativos
    // ❌ Subtotal pode ficar dessincronizado
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
