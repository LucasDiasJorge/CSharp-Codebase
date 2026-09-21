using Microsoft.Extensions.Logging;
using Modules.Abstractions.Contracts;
using Modules.Abstractions.Events;
using Modules.Catalog.Internal;

namespace Modules.Catalog;

/// <summary>
/// A fachada pública do módulo: a única classe que os outros enxergam. Implementa o
/// contrato e reage aos eventos que lhe interessam.
/// </summary>
public sealed class CatalogModule : ICatalogModule
{
    private readonly CatalogDatabase _database = new CatalogDatabase();
    private readonly IEventBus _bus;
    private readonly ILogger<CatalogModule> _logger;

    public CatalogModule(IEventBus bus, ILogger<CatalogModule> logger)
    {
        _bus = bus;
        _logger = logger;

        // O modulo se inscreve no que lhe interessa. Pedidos nao sabe que Catalogo
        // existe — apenas publica o fato de que um pedido foi feito.
        _bus.Subscribe<OrderPlaced>(OnOrderPlacedAsync);

        // Compensacao: a reserva acontece antes de o pagamento ser decidido, entao uma
        // cobranca recusada precisa devolver o estoque. Sem isto, o produto fica preso
        // a um pedido que nao existe.
        _bus.Subscribe<PaymentFailed>(OnPaymentFailedAsync);
    }

    /// <summary>Quanto foi reservado por pedido, para saber o que devolver.</summary>
    private readonly Dictionary<string, (string Sku, int Quantity)> _reservations = new Dictionary<string, (string, int)>();

    public Task<ProductInfo?> FindAsync(string sku, CancellationToken cancellationToken)
    {
        ProductEntity? product = _database.Find(sku);

        // Projeta a entidade no DTO do contrato: SupplierCode fica de fora.
        return Task.FromResult(product is null
            ? null
            : new ProductInfo(product.Sku, product.Name, product.Price, product.Stock));
    }

    public Task<IReadOnlyList<ProductInfo>> ListAsync(CancellationToken cancellationToken)
    {
        List<ProductInfo> products = new List<ProductInfo>();
        foreach (ProductEntity product in _database.All())
        {
            products.Add(new ProductInfo(product.Sku, product.Name, product.Price, product.Stock));
        }

        return Task.FromResult<IReadOnlyList<ProductInfo>>(products);
    }

    private async Task OnOrderPlacedAsync(OrderPlaced orderPlaced, CancellationToken cancellationToken)
    {
        ProductEntity? product = _database.Find(orderPlaced.Sku);

        if (product is null || !product.TryReserve(orderPlaced.Quantity))
        {
            int available = product?.Stock ?? 0;

            _logger.LogWarning(
                "Catalogo: estoque insuficiente de {Sku} para o pedido {Pedido} (pedidas {Pedidas}, disponiveis {Disponiveis}).",
                orderPlaced.Sku,
                orderPlaced.OrderId,
                orderPlaced.Quantity,
                available);

            await _bus.PublishAsync(new StockRejected(orderPlaced.Sku, orderPlaced.Quantity, available), cancellationToken)
                .ConfigureAwait(false);

            return;
        }

        _database.Save(product);
        _reservations[orderPlaced.OrderId] = (orderPlaced.Sku, orderPlaced.Quantity);

        _logger.LogInformation(
            "Catalogo: {Quantidade} unidade(s) de {Sku} reservada(s) para o pedido {Pedido}. Restam {Restante}.",
            orderPlaced.Quantity,
            orderPlaced.Sku,
            orderPlaced.OrderId,
            product.Stock);

        await _bus.PublishAsync(new StockReserved(orderPlaced.Sku, orderPlaced.Quantity), cancellationToken).ConfigureAwait(false);
    }

    private Task OnPaymentFailedAsync(PaymentFailed paymentFailed, CancellationToken cancellationToken)
    {
        if (!_reservations.Remove(paymentFailed.OrderId, out (string Sku, int Quantity) reservation))
        {
            // Nada reservado para este pedido: o estoque ja havia sido recusado antes.
            return Task.CompletedTask;
        }

        ProductEntity? product = _database.Find(reservation.Sku);
        if (product is null)
        {
            return Task.CompletedTask;
        }

        product.Release(reservation.Quantity);
        _database.Save(product);

        _logger.LogInformation(
            "Catalogo: {Quantidade} unidade(s) de {Sku} devolvida(s) ao estoque apos falha no pagamento de {Pedido}. Estoque: {Estoque}.",
            reservation.Quantity,
            reservation.Sku,
            paymentFailed.OrderId,
            product.Stock);

        return Task.CompletedTask;
    }
}
