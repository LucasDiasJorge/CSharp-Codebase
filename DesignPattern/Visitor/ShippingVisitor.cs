using System.Text;

namespace DesignPattern.Visitor;

// Computes shipping cost based on weight for DVDs and a flat rate for books
public class ShippingVisitor : IVisitor
{
    private readonly StringBuilder _sb = new();
    public decimal TotalShipping { get; private set; } = 0m;

    public void Visit(Book book)
    {
        decimal ship = 2.50m; // flat rate for books
        _sb.AppendLine($"Book: {book.Title} - Shipping: {ship:C}");
        TotalShipping += ship;
    }

    public void Visit(Dvd dvd)
    {
        // Example: R$ 5 per kg
        decimal ship = (decimal)dvd.WeightKg * 5m;
        _sb.AppendLine($"DVD: {dvd.Title} ({dvd.WeightKg}kg) - Shipping: {ship:C}");
        TotalShipping += ship;
    }

    public override string ToString()
    {
        return _sb.ToString() + $"Total shipping: {TotalShipping:C}";
    }
}