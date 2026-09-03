namespace SpanAndMemoryDemo.Models;

public sealed record TradeOrder(string Symbol, int Quantity, decimal Price)
{
    public decimal Notional => Quantity * Price;
}
