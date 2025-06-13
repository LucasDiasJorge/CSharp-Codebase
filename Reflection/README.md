# Reflection example

## 🔍 What Can You Do with Reflection?

- Discover the type of an object at runtime.
- List all methods, properties, or fields of a class.
- Dynamically invoke methods or access properties.
- Create instances of types dynamically (like a plugin system).
- Read custom attributes (like [Price] or [Required]).

## ⚠️ When to Use Reflection
Reflection is powerful, but it comes with trade-offs:
- Slower than direct code execution.
- Bypasses compile-time checks, so more prone to runtime errors.
- Best used for frameworks, tooling, or dynamic scenarios (like serialization, dependency injection, or test runners).


## Thiago crazy stuffs 

```csharp
using System;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property)]
public class PriceAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is decimal price)
            return price >= 0;
        return false;
    }
}
```

```csharp
public class ProductModel
{
    public string Name { get; set; }

    [Price]
    public decimal Price { get; set; }
}
```

```csharp
using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        var type = typeof(ProductModel);

        foreach (var prop in type.GetProperties())
        {
            var attrs = prop.GetCustomAttributes(typeof(PriceAttribute), inherit: false);
            if (attrs.Length > 0)
            {
                Console.WriteLine($"Property '{prop.Name}' has the [Price] attribute.");
            }
        }
    }
}
```

#### The output

````sh
Property 'Price' has the [Price] attribute.
```