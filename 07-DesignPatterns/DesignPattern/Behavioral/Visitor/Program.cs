using DesignPattern.Visitor;

// Very small demo of Visitor pattern
List<IElement> items = new List<IElement>
{
    new Book("C# In Depth", 49.90m),
    new Dvd("Inception", 29.90m, 0.15),
    new Book("Clean Code", 39.50m),
    new Dvd("The Matrix", 19.90m, 0.12)
};

PriceVisitor priceVisitor = new PriceVisitor();
ShippingVisitor shippingVisitor = new ShippingVisitor();

foreach (IElement item in items)
{
    item.Accept(priceVisitor);
    item.Accept(shippingVisitor);
}

Console.WriteLine("--- Price Visitor ---");
Console.WriteLine(priceVisitor.ToString());
Console.WriteLine();
Console.WriteLine("--- Shipping Visitor ---");
Console.WriteLine(shippingVisitor.ToString());

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
