# LINQ (Language Integrated Query) em C#

## 📚 Conceitos Abordados

Este projeto demonstra o uso de LINQ em C#, incluindo:

- **LINQ Syntax**: Sintaxe de consulta integrada
- **Method Syntax**: Uso de métodos de extensão
- **Query Syntax**: Sintaxe similar ao SQL
- **Deferred Execution**: Execução adiada de consultas
- **Filtering**: Filtragem de dados com Where
- **Projection**: Transformação com Select
- **Ordering**: Ordenação de resultados

## 🎯 Objetivos de Aprendizado

- Dominar consultas LINQ para manipulação de dados
- Entender diferenças entre Method e Query syntax
- Aplicar operadores LINQ comuns
- Trabalhar com diferentes data sources
- Otimizar performance de consultas

## 💡 Conceitos Importantes

### Method Syntax
```csharp
var evenNumbers = numbers.Where(num => num % 2 == 0);
```

### Query Syntax
```csharp
var evenNumbers = from num in numbers
                  where num % 2 == 0
                  select num;
```

### Deferred Execution
```csharp
var query = numbers.Where(x => x > 5); // Não executa ainda
var result = query.ToList(); // Executa agora
```

## 🚀 Como Executar

```bash
cd Linq
dotnet run
```

## 📖 O que Você Aprenderá

1. **Operadores de Filtragem**:
   - `Where()`: Filtra elementos baseado em condição
   - `OfType<T>()`: Filtra por tipo específico
   - `Distinct()`: Remove duplicatas

2. **Operadores de Projeção**:
   - `Select()`: Transforma elementos
   - `SelectMany()`: Achatamento de coleções aninhadas

3. **Operadores de Ordenação**:
   - `OrderBy()` / `OrderByDescending()`
   - `ThenBy()` / `ThenByDescending()`

4. **Operadores de Agrupamento**:
   - `GroupBy()`: Agrupa elementos por chave

## 🎨 Operadores LINQ Essenciais

### Filtering (Filtragem)
```csharp
// Where - filtragem condicional
var adults = people.Where(p => p.Age >= 18);

// Distinct - elementos únicos
var uniqueNames = names.Distinct();

// Take/Skip - paginação
var firstFive = items.Take(5);
var skipFirst = items.Skip(10);
```

### Projection (Projeção)
```csharp
// Select - transformação
var names = people.Select(p => p.Name);

// Select com objeto anônimo
var summary = people.Select(p => new { 
    Name = p.Name, 
    IsAdult = p.Age >= 18 
});

// SelectMany - achatar coleções
var allPhones = people.SelectMany(p => p.PhoneNumbers);
```

### Aggregation (Agregação)
```csharp
// Count, Sum, Average, Min, Max
var totalAge = people.Sum(p => p.Age);
var averageAge = people.Average(p => p.Age);
var oldestAge = people.Max(p => p.Age);
var count = people.Count(p => p.Age > 30);
```

### Joining (Junção)
```csharp
var result = from person in people
             join address in addresses on person.Id equals address.PersonId
             select new { person.Name, address.City };
```

### Grouping (Agrupamento)
```csharp
var grouped = people.GroupBy(p => p.City)
                   .Select(g => new { 
                       City = g.Key, 
                       Count = g.Count() 
                   });
```

## 🏗️ Casos de Uso Práticos

### 1. Processamento de Dados
```csharp
var result = orders
    .Where(o => o.Date >= DateTime.Today.AddDays(-30))
    .GroupBy(o => o.CustomerId)
    .Select(g => new {
        CustomerId = g.Key,
        TotalAmount = g.Sum(o => o.Amount),
        OrderCount = g.Count()
    })
    .OrderByDescending(x => x.TotalAmount);
```

### 2. Validação e Filtros
```csharp
var validEmails = contacts
    .Where(c => !string.IsNullOrEmpty(c.Email))
    .Where(c => c.Email.Contains("@"))
    .Select(c => c.Email.ToLower())
    .Distinct();
```

### 3. Relatórios e Estatísticas
```csharp
var stats = sales
    .GroupBy(s => s.Date.Month)
    .Select(g => new {
        Month = g.Key,
        TotalSales = g.Sum(s => s.Amount),
        AverageSale = g.Average(s => s.Amount),
        TopProduct = g.GroupBy(s => s.Product)
                      .OrderByDescending(p => p.Sum(s => s.Amount))
                      .First().Key
    });
```

## 🔍 Pontos de Atenção

### Performance
```csharp
// ❌ Múltiplas enumerações
var data = GetData().Where(x => x.IsValid);
var count = data.Count(); // Primeira enumeração
var list = data.ToList(); // Segunda enumeração

// ✅ Uma única enumeração
var data = GetData().Where(x => x.IsValid).ToList();
var count = data.Count;
```

### Null Handling
```csharp
// ❌ Pode gerar NullReferenceException
var names = people.Select(p => p.Name.ToUpper());

// ✅ Tratamento seguro de null
var names = people
    .Where(p => p.Name != null)
    .Select(p => p.Name.ToUpper());
```

### Memory Usage
```csharp
// Para grandes datasets, considere usar yield return
public static IEnumerable<T> ProcessLargeDataset<T>(IEnumerable<T> source)
{
    foreach (var item in source)
    {
        // Processamento item por item
        yield return ProcessItem(item);
    }
}
```

## 🚀 LINQ Providers

### 1. LINQ to Objects
Para coleções em memória (IEnumerable).

### 2. LINQ to SQL / Entity Framework
Para consultas em banco de dados.

### 3. LINQ to XML
Para manipulação de documentos XML.

### 4. Parallel LINQ (PLINQ)
```csharp
var result = numbers.AsParallel()
                   .Where(n => IsExpensiveOperation(n))
                   .Select(n => ProcessNumber(n));
```

## 📚 Recursos Adicionais

- [LINQ (Language-Integrated Query)](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
- [101 LINQ Samples](https://docs.microsoft.com/en-us/samples/dotnet/try-samples/101-linq-samples/)
- [LINQ Performance Tips](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/ef/language-reference/query-execution)
- [Artigo de referência](https://medium.com/@ravipatel.it/introduction-to-linq-in-c-26bf70607d14)