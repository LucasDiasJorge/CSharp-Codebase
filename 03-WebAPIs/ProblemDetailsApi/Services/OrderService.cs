using ProblemDetailsApi.Domain;
using ProblemDetailsApi.Models;

namespace ProblemDetailsApi.Services;

/// <summary>
/// Regras de negócio do exemplo. Cada caminho de falha lança a exceção de domínio
/// correspondente; a tradução para HTTP é responsabilidade dos handlers, não daqui.
/// </summary>
public sealed class OrderService
{
    private static readonly IReadOnlyDictionary<string, int> StockBySku = new Dictionary<string, int>
    {
        ["ABC-1234"] = 10,
        ["XYZ-9999"] = 0
    };

    private readonly Dictionary<int, DateTimeOffset> _paidOrders = new Dictionary<int, DateTimeOffset>
    {
        [2] = new DateTimeOffset(2026, 3, 14, 10, 30, 0, TimeSpan.Zero)
    };

    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public OrderResponse Create(CreateOrderRequest request)
    {
        string sku = request.Sku!;

        if (!StockBySku.TryGetValue(sku, out int available))
        {
            throw new InsufficientStockException(sku, request.Quantity, 0);
        }

        if (request.Quantity > available)
        {
            throw new InsufficientStockException(sku, request.Quantity, available);
        }

        _logger.LogInformation("Pedido criado para o SKU {Sku} com quantidade {Quantidade}.", sku, request.Quantity);

        return new OrderResponse(100, sku, request.Quantity, "criado");
    }

    public OrderResponse GetById(int id)
    {
        if (id is not (1 or 2))
        {
            throw new OrderNotFoundException(id);
        }

        return new OrderResponse(id, "ABC-1234", 2, _paidOrders.ContainsKey(id) ? "pago" : "aberto");
    }

    public OrderResponse Pay(int id)
    {
        if (id is not (1 or 2))
        {
            throw new OrderNotFoundException(id);
        }

        if (_paidOrders.TryGetValue(id, out DateTimeOffset paidAt))
        {
            throw new OrderAlreadyPaidException(id, paidAt);
        }

        _paidOrders[id] = DateTimeOffset.UtcNow;
        _logger.LogInformation("Pedido {PedidoId} pago.", id);

        return new OrderResponse(id, "ABC-1234", 2, "pago");
    }

    public void Explode()
    {
        // Falha não prevista: nenhum handler de domínio a reconhece.
        throw new InvalidOperationException("Falha inesperada em dependencia interna.");
    }
}
