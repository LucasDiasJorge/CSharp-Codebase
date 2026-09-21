namespace Modules.Abstractions.Contracts;

/// <summary>
/// Contrato público do módulo de Catálogo. É a única superfície que os outros módulos
/// enxergam — as entidades, o repositório e as regras internas ficam inacessíveis
/// porque são <c>internal</c> ao assembly do módulo.
/// </summary>
public interface ICatalogModule
{
    Task<ProductInfo?> FindAsync(string sku, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductInfo>> ListAsync(CancellationToken cancellationToken);
}

/// <summary>
/// DTO do contrato. Não é a entidade do catálogo: é uma projeção do que os outros
/// módulos têm direito de saber. Expor a entidade acoplaria todo mundo ao modelo interno.
/// </summary>
public sealed record ProductInfo(string Sku, string Name, decimal Price, int AvailableStock);

/// <summary>Contrato público do módulo de Pedidos.</summary>
public interface IOrdersModule
{
    Task<string> PlaceOrderAsync(string customerId, string sku, int quantity, CancellationToken cancellationToken);

    Task<OrderInfo?> FindAsync(string orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderInfo>> ListAsync(CancellationToken cancellationToken);
}

public sealed record OrderInfo(string OrderId, string CustomerId, string Sku, int Quantity, decimal Total, string Status);

/// <summary>Contrato público do módulo de Pagamentos.</summary>
public interface IPaymentsModule
{
    Task<IReadOnlyList<PaymentInfo>> ListAsync(CancellationToken cancellationToken);
}

public sealed record PaymentInfo(string OrderId, decimal Amount, string Status);
