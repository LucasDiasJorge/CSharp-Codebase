namespace DesignPattern.Visitor;

public class Book : IElement
{
    public string Title { get; }
    public decimal Price { get; }

    public Book(string title, decimal price)
    {
        Title = title;
        Price = price;
    }

    public void Accept(IVisitor visitor) => visitor.Visit(this);
}