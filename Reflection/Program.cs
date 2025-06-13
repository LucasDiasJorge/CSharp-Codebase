using System;
using System.Reflection;

public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }

    public void PrintInfo() => Console.WriteLine($"{Name}: {Price:C}");
}

class Program
{
    static void Main()
    {
        var product = new Product { Name = "Laptop", Price = 1500m };

        Type type = product.GetType();
        Console.WriteLine("Type: " + type.Name);

        foreach (var prop in type.GetProperties())
        {
            Console.WriteLine($"Property: {prop.Name} = {prop.GetValue(product)}");
        }

        MethodInfo method = type.GetMethod("PrintInfo");
        method.Invoke(product, null);
    }
}