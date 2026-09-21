using Microsoft.Extensions.Logging;
using Modules.Abstractions.Contracts;
using Modules.Abstractions.Events;
using Modules.Orders.Internal;

namespace Modules.Orders;

/// <summary>
/// Módulo de Pedidos. Ele conhece o **contrato** do Catálogo (para consultar preço),
/// mas não o seu banco nem as suas entidades. E não conhece Pagamentos de forma alguma:
/// apenas publica que um pedido foi feito e reage ao que voltar.
/// </summary>
public sealed class OrdersModule : IOrdersModule
{
    private readonly OrdersDatabase _database = new OrdersDatabase();
    private readonly ICatalogModule _catalog;
    private readonly IEventBus _bus;
    private readonly ILogger<OrdersModule> _logger;

    private int _sequence;

    public OrdersModule(ICatalogModule catalog, IEventBus bus, ILogger<OrdersModule> logger)
    {
        _catalog = catalog;
        _bus = bus;
        _logger = logger;

        _bus.Subscribe<StockReserved>(OnStockReservedAsync);
        _bus.Subscribe<StockRejected>(OnStockRejectedAsync);
        _bus.Subscribe<PaymentConfirmed>(OnPaymentConfirmedAsync);
        _bus.Subscribe<PaymentFailed>(OnPaymentFailedAsync);
    }

    public async Task<string> PlaceOrderAsync(string customerId, string sku, int quantity, CancellationToken cancellationToken)
    {
        // Chamada SINCRONA pelo contrato publico: precisamos do preco agora, e uma
        // consulta direta e mais simples e mais honesta que um evento de ida e volta.
        ProductInfo? product = await _catalog.FindAsync(sku, cancellationToken).ConfigureAwait(false);

        if (product is null)
        {
            throw new InvalidOperationException($"Produto {sku} nao existe no catalogo.");
        }

        string orderId = $"PED-{Interlocked.Increment(ref _sequence):D3}";
        OrderEntity order = new OrderEntity(orderId, customerId, sku, quantity, product.Price * quantity);

        _database.Save(order);

        _logger.LogInformation("Pedidos: {Pedido} criado para {Cliente} ({Quantidade}x {Sku}, total {Total:F2}).",
            orderId, customerId, quantity, sku, order.Total);

        // Publicacao ASSINCRONA: Pedidos nao sabe (nem precisa saber) que Catalogo e
        // Pagamentos vao reagir a isto.
        await _bus.PublishAsync(new OrderPlaced(orderId, customerId, sku, quantity, order.Total), cancellationToken)
            .ConfigureAwait(false);

        return orderId;
    }

    public Task<OrderInfo?> FindAsync(string orderId, CancellationToken cancellationToken)
    {
        OrderEntity? order = _database.Find(orderId);

        return Task.FromResult(order is null ? null : Project(order));
    }

    public Task<IReadOnlyList<OrderInfo>> ListAsync(CancellationToken cancellationToken)
    {
        List<OrderInfo> orders = new List<OrderInfo>();
        foreach (OrderEntity order in _database.All())
        {
            orders.Add(Project(order));
        }

        return Task.FromResult<IReadOnlyList<OrderInfo>>(orders);
    }

    private static OrderInfo Project(OrderEntity order) =>
        new OrderInfo(order.OrderId, order.CustomerId, order.Sku, order.Quantity, order.Total, order.Status);

    private Task OnStockReservedAsync(StockReserved stockReserved, CancellationToken cancellationToken)
    {
        // StockReserved nao carrega o id do pedido: o Catalogo reserva por SKU. Encontrar
        // o pedido correspondente e responsabilidade de quem escuta.
        foreach (OrderEntity order in _database.All())
        {
            if (order.Sku == stockReserved.Sku && !order.StockReserved)
            {
                order.MarkStockReserved();
                _logger.LogInformation("Pedidos: {Pedido} com estoque reservado. Status: {Status}.", order.OrderId, order.Status);

                break;
            }
        }

        return Task.CompletedTask;
    }

    private Task OnStockRejectedAsync(StockRejected stockRejected, CancellationToken cancellationToken)
    {
        foreach (OrderEntity order in _database.All())
        {
            if (order.Sku == stockRejected.Sku && order.Status == "aguardando")
            {
                order.Fail("sem estoque");
                _logger.LogWarning("Pedidos: {Pedido} cancelado por falta de estoque.", order.OrderId);

                break;
            }
        }

        return Task.CompletedTask;
    }

    private Task OnPaymentConfirmedAsync(PaymentConfirmed payment, CancellationToken cancellationToken)
    {
        OrderEntity? order = _database.Find(payment.OrderId);
        if (order is not null)
        {
            order.MarkPaid();
            _logger.LogInformation("Pedidos: {Pedido} com pagamento confirmado. Status: {Status}.", order.OrderId, order.Status);
        }

        return Task.CompletedTask;
    }

    private Task OnPaymentFailedAsync(PaymentFailed payment, CancellationToken cancellationToken)
    {
        OrderEntity? order = _database.Find(payment.OrderId);
        if (order is not null)
        {
            order.Fail(payment.Reason);
            _logger.LogWarning("Pedidos: {Pedido} cancelado: {Motivo}.", order.OrderId, payment.Reason);
        }

        return Task.CompletedTask;
    }
}
