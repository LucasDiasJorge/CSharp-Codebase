namespace GoodOrderApi.Api.DTOs;

// DTOs para a API - Separados do domínio

public sealed class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerZip { get; set; } = string.Empty;
    public List<OrderItemRequest> Items { get; set; } = new();
    public string? DiscountCode { get; set; }
}

public sealed class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class PaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
}

public sealed class OrderResponse
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Discount { get; set; }
    public string? DiscountCode { get; set; }
    public bool IsPaid { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public sealed class OrderItemResponse
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public sealed class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class ReportResponse
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscount { get; set; }
    public int TotalItems { get; set; }
    public int PaidOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public sealed class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}

public sealed class SuccessResponse
{
    public string Message { get; set; } = string.Empty;
}
