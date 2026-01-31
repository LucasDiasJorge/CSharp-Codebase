using GoodOrderApi.Domain.Entities;
using GoodOrderApi.Application.Services;

namespace GoodOrderApi.Api.DTOs;

// ✅ REGRA 5: Um ponto por linha - Mappers extraem lógica de mapeamento

/// <summary>
/// Mapper para converter entre DTOs e entidades de domínio.
/// </summary>
public static class OrderMapper
{
    public static CreateOrderCommand ToCommand(CreateOrderRequest request)
    {
        return new CreateOrderCommand
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            CustomerAddress = request.CustomerAddress,
            CustomerCity = request.CustomerCity,
            CustomerZip = request.CustomerZip,
            Items = request.Items.Select(ToItemCommand).ToList(),
            DiscountCode = request.DiscountCode
        };
    }

    private static OrderItemCommand ToItemCommand(OrderItemRequest request)
    {
        return new OrderItemCommand
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };
    }

    public static OrderResponse ToResponse(Order order)
    {
        var response = new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerData.Customer.Name.ToString(),
            Status = order.State.Status.Name,
            Total = order.Financials.CalculateTotal().Amount,
            Discount = order.Financials.Discount.Amount.Amount,
            DiscountCode = order.Financials.Discount.Code,
            IsPaid = order.State.IsPaid,
            CreatedAt = order.CustomerData.CreatedAt,
            Items = MapItems(order)
        };

        return response;
    }

    private static List<OrderItemResponse> MapItems(Order order)
    {
        return order.Financials.Items
            .ToList()
            .Select(MapItem)
            .ToList();
    }

    private static OrderItemResponse MapItem(OrderItem item)
    {
        return new OrderItemResponse
        {
            ProductName = item.Details.Product.Name,
            Quantity = item.Details.Quantity.Value,
            UnitPrice = item.Details.Product.Price.Amount,
            TotalPrice = item.CalculateTotal().Amount
        };
    }

    public static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Info.Identification.Name,
            Description = product.Info.Identification.Description,
            Price = product.Info.Pricing.Price.Amount,
            StockQuantity = product.Inventory.StockQuantity.Value,
            Category = product.Info.Pricing.Category,
            IsActive = product.Inventory.IsActive
        };
    }

    public static ReportResponse ToResponse(OrderReport report)
    {
        return new ReportResponse
        {
            TotalOrders = report.Summary.Items.TotalOrders,
            TotalRevenue = report.Summary.Financial.TotalRevenue.Amount,
            TotalDiscount = report.Summary.Financial.TotalDiscount.Amount,
            TotalItems = report.Summary.Items.TotalItems,
            PaidOrders = report.Counts.PaidOrders,
            PendingOrders = report.Counts.PendingOrders,
            CancelledOrders = report.Counts.CancelledOrders,
            AverageOrderValue = report.Summary.Financial.AverageOrderValue(report.Summary.Items.TotalOrders).Amount
        };
    }
}
