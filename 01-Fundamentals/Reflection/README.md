# Reflection em C#

## 📚 Conceitos Abordados

Este projeto demonstra o uso de Reflection em C#, incluindo:

- **Type Information**: Obtenção de informações sobre tipos
- **Property Access**: Acesso a propriedades via reflexão
- **Method Invocation**: Invocação de métodos dinamicamente
- **Assembly Loading**: Carregamento dinâmico de assemblies
- **Attribute Reading**: Leitura de atributos customizados
- **Dynamic Object Creation**: Criação de objetos em runtime

## 🎯 Objetivos de Aprendizado

- Entender como inspecionar tipos em runtime
- Acessar membros de classes dinamicamente
- Invocar métodos sem conhecer o tipo em compile-time
- Trabalhar com metadados de assemblies
- Criar frameworks flexíveis usando reflection

## � Conceitos Importantes

### Obtendo Type Information
```csharp
Type type = typeof(Product);
Type type2 = product.GetType();
Type type3 = Type.GetType("MyNamespace.Product");
```

### Acessando Propriedades
```csharp
PropertyInfo[] properties = type.GetProperties();
foreach (var prop in properties)
{
    var value = prop.GetValue(instance);
    Console.WriteLine($"{prop.Name}: {value}");
}
```

### Invocando Métodos
```csharp
MethodInfo method = type.GetMethod("MethodName");
object result = method.Invoke(instance, parameters);
```

## 🚀 Como Executar

```bash
cd Reflection
dotnet run
```

## �🔍 What Can You Do with Reflection?

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

## 📖 O que Você Aprenderá

1. **Metadados de Tipo**:
   - Nome do tipo, namespace, assembly
   - Hierarquia de herança
   - Interfaces implementadas
   - Modificadores de acesso

2. **Membros de Classe**:
   - Propriedades e campos
   - Métodos e construtores
   - Eventos e delegates

3. **Criação Dinâmica**:
   - Instanciação de objetos
   - Chamada de construtores
   - Configuração de propriedades

4. **Atributos Customizados**:
   - Definição de atributos
   - Leitura de metadados
   - Validação baseada em atributos

## 🎨 Casos de Uso Práticos

### 1. Object Mapping
```csharp
public static T MapProperties<T>(object source) where T : new()
{
    var result = new T();
    var sourceType = source.GetType();
    var targetType = typeof(T);
    
    foreach (var sourceProp in sourceType.GetProperties())
    {
        var targetProp = targetType.GetProperty(sourceProp.Name);
        if (targetProp != null && targetProp.CanWrite)
        {
            var value = sourceProp.GetValue(source);
            targetProp.SetValue(result, value);
        }
    }
    
    return result;
}
```

### 2. Validation Framework
```csharp
public static ValidationResult Validate(object obj)
{
    var type = obj.GetType();
    var errors = new List<string>();
    
    foreach (var prop in type.GetProperties())
    {
        var requiredAttr = prop.GetCustomAttribute<RequiredAttribute>();
        if (requiredAttr != null)
        {
            var value = prop.GetValue(obj);
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                errors.Add($"{prop.Name} is required");
            }
        }
    }
    
    return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
}
```

### 3. Plugin Architecture
```csharp
public static IEnumerable<T> LoadPlugins<T>(string pluginDirectory)
{
    var pluginType = typeof(T);
    var plugins = new List<T>();
    
    foreach (var file in Directory.GetFiles(pluginDirectory, "*.dll"))
    {
        var assembly = Assembly.LoadFrom(file);
        var types = assembly.GetTypes()
            .Where(t => pluginType.IsAssignableFrom(t) && !t.IsInterface);
            
        foreach (var type in types)
        {
            var plugin = (T)Activator.CreateInstance(type);
            plugins.Add(plugin);
        }
    }
    
    return plugins;
}
```

## 🔍 Pontos de Atenção

### Performance
```csharp
// ❌ Reflexão é lenta - evite em loops
for (int i = 0; i < 1000000; i++)
{
    var method = type.GetMethod("Execute"); // Lento!
    method.Invoke(obj, null);
}

// ✅ Cache informações de reflexão
var method = type.GetMethod("Execute"); // Uma vez só
for (int i = 0; i < 1000000; i++)
{
    method.Invoke(obj, null); // Melhor
}
```

### Security
```csharp
// ⚠️ Cuidado com acesso a membros privados
var privateField = type.GetField("_secretData", 
    BindingFlags.NonPublic | BindingFlags.Instance);
```

## 📚 Recursos Adicionais

- [Reflection in .NET](https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/reflection)
- [Expression Trees](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/)
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