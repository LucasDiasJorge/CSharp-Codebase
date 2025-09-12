# Visitor Pattern Example

Simple and intuitive demo of the Visitor pattern.

Structure:
- `IElement` — interface for elements that accept visitors.
- `IVisitor` — visitor interface with visit methods for each concrete element.
- `Book`, `Dvd` — concrete elements.
- `PriceVisitor` — computes total price.
- `ShippingVisitor` — computes shipping cost.

Run:
```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\DesignPattern\Visitor"
dotnet run
```

This prints a breakdown from both visitors and a total for each.
