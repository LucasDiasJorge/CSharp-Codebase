using GoodOrderApi.Domain.Entities;
using GoodOrderApi.Domain.ValueObjects;

namespace GoodOrderApi.Application.Services;

// ✅ REGRA 7: Classes pequenas e focadas
// ✅ REGRA 1: Um nível de indentação

/// <summary>
/// Serviço para geração de relatórios.
/// Separado do OrderApplicationService para manter responsabilidade única.
/// </summary>
public class ReportService
{
    private readonly OrderRepository _orderRepository;

    public ReportService(OrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public OrderReport GenerateReport(DateTime? startDate, DateTime? endDate)
    {
        var orders = GetFilteredOrders(startDate, endDate);
        return OrderReport.Generate(orders);
    }

    private IEnumerable<Order> GetFilteredOrders(DateTime? startDate, DateTime? endDate)
    {
        var orders = _orderRepository.GetAll().AsEnumerable();
        
        if (startDate.HasValue)
            orders = orders.Where(o => o.CustomerData.CreatedAt >= startDate.Value);
        
        if (endDate.HasValue)
            orders = orders.Where(o => o.CustomerData.CreatedAt <= endDate.Value);
        
        return orders;
    }
}

/// <summary>
/// Value Object que representa um relatório de pedidos.
/// ✅ REGRA 7: Classe pequena e coesa
/// </summary>
public sealed class OrderReport
{
    public ReportSummary Summary { get; }
    public ReportCounts Counts { get; }

    private OrderReport(ReportSummary summary, ReportCounts counts)
    {
        Summary = summary;
        Counts = counts;
    }

    public static OrderReport Generate(IEnumerable<Order> orders)
    {
        var orderList = orders.ToList();
        var summary = ReportSummary.Calculate(orderList);
        var counts = ReportCounts.Calculate(orderList);
        return new OrderReport(summary, counts);
    }
}

/// <summary>
/// ✅ REGRA 8: Máximo duas variáveis de instância
/// </summary>
public sealed class ReportSummary
{
    public FinancialSummary Financial { get; }
    public ItemsSummary Items { get; }

    private ReportSummary(FinancialSummary financial, ItemsSummary items)
    {
        Financial = financial;
        Items = items;
    }

    public static ReportSummary Calculate(List<Order> orders)
    {
        var financial = FinancialSummary.Calculate(orders);
        var items = ItemsSummary.Calculate(orders);
        return new ReportSummary(financial, items);
    }
}

public sealed class FinancialSummary
{
    public Money TotalRevenue { get; }
    public Money TotalDiscount { get; }

    private FinancialSummary(Money totalRevenue, Money totalDiscount)
    {
        TotalRevenue = totalRevenue;
        TotalDiscount = totalDiscount;
    }

    public static FinancialSummary Calculate(List<Order> orders)
    {
        var revenue = orders.Aggregate(Money.Zero, (sum, o) => sum.Add(o.Financials.CalculateTotal()));
        var discount = orders.Aggregate(Money.Zero, (sum, o) => sum.Add(o.Financials.Discount.Amount));
        return new FinancialSummary(revenue, discount);
    }

    public Money AverageOrderValue(int orderCount)
    {
        if (orderCount == 0) return Money.Zero;
        return Money.Create(TotalRevenue.Amount / orderCount);
    }
}

public sealed class ItemsSummary
{
    public int TotalItems { get; }
    public int TotalOrders { get; }

    private ItemsSummary(int totalItems, int totalOrders)
    {
        TotalItems = totalItems;
        TotalOrders = totalOrders;
    }

    public static ItemsSummary Calculate(List<Order> orders)
    {
        var items = orders.Sum(o => o.Financials.Items.CalculateTotalQuantity());
        return new ItemsSummary(items, orders.Count);
    }
}

public sealed class ReportCounts
{
    public int PaidOrders { get; }
    public int PendingOrders { get; }
    public int CancelledOrders { get; }

    private ReportCounts(int paidOrders, int pendingOrders, int cancelledOrders)
    {
        PaidOrders = paidOrders;
        PendingOrders = pendingOrders;
        CancelledOrders = cancelledOrders;
    }

    public static ReportCounts Calculate(List<Order> orders)
    {
        var cancelled = orders.Count(o => o.State.Status is CancelledStatus);
        var paid = orders.Count(o => o.State.IsPaid && o.State.Status is not CancelledStatus);
        var pending = orders.Count(o => !o.State.IsPaid && o.State.Status is not CancelledStatus);
        
        return new ReportCounts(paid, pending, cancelled);
    }
}
