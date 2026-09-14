namespace ServerSentEventsDemo.Events;

/// <summary>
/// Evento publicado no fluxo. O <see cref="Id"/> é sequencial e global — é ele que vai
/// no campo <c>id:</c> do SSE e volta como <c>Last-Event-ID</c> quando o cliente
/// reconecta.
/// </summary>
public sealed class PriceTick
{
    public PriceTick(long id, string symbol, decimal price, DateTimeOffset occurredAt)
    {
        Id = id;
        Symbol = symbol;
        Price = price;
        OccurredAt = occurredAt;
    }

    public long Id { get; }

    public string Symbol { get; }

    public decimal Price { get; }

    public DateTimeOffset OccurredAt { get; }
}
