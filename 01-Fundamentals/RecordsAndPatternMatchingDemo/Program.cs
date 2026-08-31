using System;
using System.Linq;

namespace RecordsAndPatternMatchingDemo;

public static class Program
{
    public static void Main()
    {
        Customer originalCustomer = new("CUS-001", "Ana Lima", "Gold", new Address("Sao Paulo", "SP"));
        Customer sameCustomer = new("CUS-001", "Ana Lima", "Gold", new Address("Sao Paulo", "SP"));
        Customer updatedCustomer = originalCustomer with { Tier = "Platinum" };

        OrderLine[] orderLines =
        [
            new("BK-001", "Clean Code", 1, 120m),
            new("BK-002", "C# in Depth", 2, 180m),
            new("SHIP", "Frete", 1, 0m)
        ];

        Order order = new("ORD-1001", updatedCustomer, orderLines, "Pending");
        Order approvedOrder = order with { Status = "Approved" };

        Console.WriteLine("Records and Pattern Matching Demo");
        Console.WriteLine();

        PrintValueEquality(originalCustomer, sameCustomer, updatedCustomer);
        PrintOrderSnapshot(order, approvedOrder);
        PrintPatternMatching(order);
    }

    private static void PrintValueEquality(Customer originalCustomer, Customer sameCustomer, Customer updatedCustomer)
    {
        Console.WriteLine("== Igualdade por valor ==");
        Console.WriteLine($"originalCustomer == sameCustomer: {originalCustomer == sameCustomer}");
        Console.WriteLine($"originalCustomer == updatedCustomer: {originalCustomer == updatedCustomer}");
        Console.WriteLine($"Original tier: {originalCustomer.Tier}");
        Console.WriteLine($"Novo tier via with: {updatedCustomer.Tier}");
        Console.WriteLine();
    }

    private static void PrintOrderSnapshot(Order order, Order approvedOrder)
    {
        Console.WriteLine("== Copia imutavel com with ==");
        Console.WriteLine($"Pedido original: {order.Id} - {order.Status}");
        Console.WriteLine($"Pedido aprovado: {approvedOrder.Id} - {approvedOrder.Status}");
        Console.WriteLine($"Mesmo cliente reaproveitado: {ReferenceEquals(order.Customer, approvedOrder.Customer)}");
        Console.WriteLine();
    }

    private static void PrintPatternMatching(Order order)
    {
        Console.WriteLine("== Pattern matching ==");
        Console.WriteLine($"Posicional: {DescribeCustomerLocation(order.Customer)}");
        Console.WriteLine($"Por propriedade: {DescribeApprovalRisk(order)}");
        Console.WriteLine($"Por lista: {DescribeLines(order.Lines)}");
    }

    private static string DescribeCustomerLocation(Customer customer)
    {
        string description = customer switch
        {
            Customer(_, _, "Platinum", Address("Sao Paulo", "SP")) => "cliente Platinum de Sao Paulo",
            Customer(_, _, "Gold", Address(_, "SP")) => "cliente Gold do estado de Sao Paulo",
            Customer(_, string name, _, Address(string city, _)) => $"{name} mora em {city}",
            _ => "cliente sem classificacao"
        };

        return description;
    }

    private static string DescribeApprovalRisk(Order order)
    {
        string description = order switch
        {
            { Status: "Pending", Total: >= 400m, Customer.Tier: "Platinum" } => "aprovar com prioridade",
            { Status: "Pending", Total: >= 400m } => "revisar pedido de alto valor",
            { Status: "Pending", Lines.Length: 0 } => "pedido vazio",
            { Status: "Approved" } => "pedido ja aprovado",
            _ => "fluxo padrao"
        };

        return description;
    }

    private static string DescribeLines(OrderLine[] lines)
    {
        string description = lines switch
        {
            [] => "pedido sem itens",
            [OrderLine(_, string name, 1, _)] => $"apenas um item: {name}",
            [.., OrderLine("SHIP", _, _, 0m)] => "pedido com frete gratis no final",
            [OrderLine(_, string firstName, >= 2, _), ..] => $"primeiro item com quantidade relevante: {firstName}",
            _ => "itens em formato comum"
        };

        return description;
    }
}

public sealed record Address(string City, string State);

public sealed record Customer(string Id, string Name, string Tier, Address Address);

public sealed record OrderLine(string Sku, string Name, int Quantity, decimal UnitPrice)
{
    public decimal Subtotal => Quantity * UnitPrice;
}

public sealed record Order(string Id, Customer Customer, OrderLine[] Lines, string Status)
{
    public decimal Total => Lines.Sum(static (OrderLine line) => line.Subtotal);
}
