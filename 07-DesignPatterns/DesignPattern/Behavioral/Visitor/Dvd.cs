namespace DesignPattern.Visitor;

public class Dvd : IElement
{
    public string Title { get; }
    public decimal Price { get; }
    public double WeightKg { get; }

    public Dvd(string title, decimal price, double weightKg)
    {
        Title = title;
        Price = price;
        WeightKg = weightKg;
    }

    public void Accept(IVisitor visitor) => visitor.Visit(this);
}