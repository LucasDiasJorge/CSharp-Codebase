namespace Modules.Abstractions.Events;

/// <summary>
/// Evento de integração entre módulos. É o único jeito de um módulo comunicar algo aos
/// outros sem conhecê-los — quem publica não sabe quem escuta, e é isso que mantém a
/// dependência em uma direção só.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Publicado pelo módulo de Pedidos. Repare que ele carrega os dados que os outros
/// módulos precisam — não um id para eles irem buscar na tabela alheia.
/// </summary>
public sealed record OrderPlaced(string OrderId, string CustomerId, string Sku, int Quantity, decimal Total) : IntegrationEvent;

/// <summary>Publicado pelo módulo de Pagamentos.</summary>
public sealed record PaymentConfirmed(string OrderId, decimal Amount) : IntegrationEvent;

/// <summary>Publicado pelo módulo de Pagamentos quando a cobrança falha.</summary>
public sealed record PaymentFailed(string OrderId, string Reason) : IntegrationEvent;

/// <summary>Publicado pelo módulo de Catálogo.</summary>
public sealed record StockReserved(string Sku, int Quantity) : IntegrationEvent;

/// <summary>Publicado pelo módulo de Catálogo quando não há estoque.</summary>
public sealed record StockRejected(string Sku, int Requested, int Available) : IntegrationEvent;
