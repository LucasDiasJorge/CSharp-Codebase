using System.Text;

namespace DesignPattern.Visitor;

// Calculates and returns the total price for the visited elements
public class PriceVisitor : IVisitor
{
    private readonly StringBuilder _sb = new();
    public decimal Total { get; private set; } = 0m;

    public void Visit(Book book)
    {
        _sb.AppendLine($"Book: {book.Title} - {book.Price:C}");
        Total += book.Price;
    }

    public void Visit(Dvd dvd)
    {
        _sb.AppendLine($"DVD: {dvd.Title} - {dvd.Price:C}");
        Total += dvd.Price;
    }

    public override string ToString()
    {
        return _sb.ToString() + $"Total: {Total:C}";
    }
}