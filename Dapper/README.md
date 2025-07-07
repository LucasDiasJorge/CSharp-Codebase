# Dapper - Micro ORM para .NET

## 📚 Conceitos Abordados

Este projeto demonstra o uso do Dapper, um micro ORM para .NET:

- **Micro ORM**: ORM leve e performático
- **Raw SQL**: Execução de queries SQL puras
- **Parameter Binding**: Vinculação segura de parâmetros
- **Object Mapping**: Mapeamento automático para objetos
- **Multiple Queries**: Execução de múltiplas consultas
- **Stored Procedures**: Chamada de procedures
- **Performance**: Otimização de consultas

## 🎯 Objetivos de Aprendizado

- Entender as vantagens do Dapper sobre ORMs pesados
- Executar queries SQL de forma eficiente
- Mapear resultados para objetos .NET
- Implementar operações CRUD performáticas
- Trabalhar com stored procedures
- Otimizar acesso a dados

## 💡 Conceitos Importantes

### Consulta Simples
```csharp
var products = connection.Query<Product>(
    "SELECT Id, Name, Price FROM Products WHERE Price > @minPrice",
    new { minPrice = 100 }
);
```

### Consulta Única
```csharp
var product = connection.QuerySingleOrDefault<Product>(
    "SELECT * FROM Products WHERE Id = @id",
    new { id = productId }
);
```

### Inserção
```csharp
var sql = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)";
connection.Execute(sql, new { Name = "Product", Price = 99.99 });
```

## 🚀 Como Executar

```bash
cd Dapper
dotnet run
```

## 📖 O que Você Aprenderá

1. **Vantagens do Dapper**:
   - Performance superior aos ORMs pesados
   - Controle total sobre SQL
   - Sintaxe simples e intuitiva
   - Suporte a SQL complexo

2. **Operações Básicas**:
   - Query: Consultas que retornam múltiplos resultados
   - QuerySingle: Consulta que retorna um único resultado
   - Execute: Execução de comandos sem retorno
   - ExecuteScalar: Execução que retorna valor único

3. **Mapeamento de Objetos**:
   - Mapeamento automático por nome
   - Mapeamento customizado
   - Tipos anônimos
   - Tipos dinâmicos

4. **Parâmetros**:
   - Objetos anônimos
   - DynamicParameters
   - Prevenção de SQL Injection

## 🎨 Padrões de Implementação

### 1. Repository Pattern
```csharp
public class ProductRepository
{
    private readonly IDbConnection _connection;
    
    public ProductRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _connection.QueryAsync<Product>(
            "SELECT Id, Name, Price, CategoryId FROM Products"
        );
    }
    
    public async Task<Product> GetByIdAsync(int id)
    {
        return await _connection.QuerySingleOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id",
            new { Id = id }
        );
    }
    
    public async Task<int> CreateAsync(Product product)
    {
        var sql = @"
            INSERT INTO Products (Name, Price, CategoryId) 
            VALUES (@Name, @Price, @CategoryId);
            SELECT CAST(SCOPE_IDENTITY() as int);";
            
        return await _connection.QuerySingleAsync<int>(sql, product);
    }
    
    public async Task<bool> UpdateAsync(Product product)
    {
        var sql = @"
            UPDATE Products 
            SET Name = @Name, Price = @Price, CategoryId = @CategoryId 
            WHERE Id = @Id";
            
        var rowsAffected = await _connection.ExecuteAsync(sql, product);
        return rowsAffected > 0;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var sql = "DELETE FROM Products WHERE Id = @Id";
        var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}
```

### 2. Multiple Results
```csharp
public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
    int page, int pageSize)
{
    var sql = @"
        SELECT * FROM Products 
        ORDER BY Id 
        OFFSET @Offset ROWS 
        FETCH NEXT @PageSize ROWS ONLY;
        
        SELECT COUNT(*) FROM Products;";
    
    using var multi = await _connection.QueryMultipleAsync(sql, new 
    { 
        Offset = (page - 1) * pageSize, 
        PageSize = pageSize 
    });
    
    var products = await multi.ReadAsync<Product>();
    var totalCount = await multi.ReadSingleAsync<int>();
    
    return (products, totalCount);
}
```

### 3. Join Queries
```csharp
public async Task<IEnumerable<ProductWithCategory>> GetProductsWithCategoryAsync()
{
    var sql = @"
        SELECT p.Id, p.Name, p.Price, 
               c.Id as CategoryId, c.Name as CategoryName
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id";
    
    return await _connection.QueryAsync<ProductWithCategory>(sql);
}

// Ou usando multi-mapping
public async Task<IEnumerable<Product>> GetProductsWithCategoryMappedAsync()
{
    var sql = @"
        SELECT p.*, c.*
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id";
    
    var productDict = new Dictionary<int, Product>();
    
    var products = await _connection.QueryAsync<Product, Category, Product>(
        sql,
        (product, category) =>
        {
            if (!productDict.TryGetValue(product.Id, out var existingProduct))
            {
                existingProduct = product;
                productDict.Add(product.Id, existingProduct);
            }
            existingProduct.Category = category;
            return existingProduct;
        },
        splitOn: "Id"
    );
    
    return productDict.Values;
}
```

### 4. Stored Procedures
```csharp
public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
{
    return await _connection.QueryAsync<Product>(
        "GetProductsByCategory",
        new { CategoryId = categoryId },
        commandType: CommandType.StoredProcedure
    );
}

public async Task<ProductStatistics> GetProductStatisticsAsync()
{
    return await _connection.QuerySingleAsync<ProductStatistics>(
        "GetProductStatistics",
        commandType: CommandType.StoredProcedure
    );
}
```

## 🏗️ Configuração e Setup

### 1. Dependency Injection
```csharp
// Program.cs
builder.Services.AddScoped<IDbConnection>(provider =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

### 2. Transaction Management
```csharp
public async Task<bool> TransferProductsAsync(int fromCategoryId, int toCategoryId)
{
    using var transaction = _connection.BeginTransaction();
    try
    {
        await _connection.ExecuteAsync(
            "UPDATE Products SET CategoryId = @ToCategoryId WHERE CategoryId = @FromCategoryId",
            new { FromCategoryId = fromCategoryId, ToCategoryId = toCategoryId },
            transaction
        );
        
        await _connection.ExecuteAsync(
            "INSERT INTO CategoryTransfers (FromId, ToId, TransferDate) VALUES (@FromId, @ToId, @Date)",
            new { FromId = fromCategoryId, ToId = toCategoryId, Date = DateTime.Now },
            transaction
        );
        
        transaction.Commit();
        return true;
    }
    catch
    {
        transaction.Rollback();
        return false;
    }
}
```

### 3. Custom Type Handlers
```csharp
public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = JsonSerializer.Serialize(value);
    }

    public override T Parse(object value)
    {
        return JsonSerializer.Deserialize<T>(value.ToString());
    }
}

// Registrar no startup
SqlMapper.AddTypeHandler(new JsonTypeHandler<List<string>>());
```

## 🔍 Pontos de Atenção

### Performance
```csharp
// ✅ Use async para operações I/O
var products = await connection.QueryAsync<Product>(sql);

// ✅ Reutilize connections quando possível
using var connection = new SqlConnection(connectionString);

// ✅ Use parâmetros para evitar SQL injection
var product = await connection.QuerySingleAsync<Product>(
    "SELECT * FROM Products WHERE Id = @id",
    new { id = productId } // ✅ Parametrizado
);
```

### Memory Management
```csharp
// ✅ Para grandes datasets, use QueryUnbuffered
var largeResult = connection.QueryUnbuffered<Product>(
    "SELECT * FROM Products" // Não carrega tudo na memória
);

foreach (var product in largeResult)
{
    // Processa item por item
}
```

### Error Handling
```csharp
try
{
    var result = await connection.QueryAsync<Product>(sql, parameters);
    return result;
}
catch (SqlException ex)
{
    // Log da exceção SQL específica
    _logger.LogError(ex, "SQL Error executing query: {Query}", sql);
    throw;
}
catch (InvalidOperationException ex)
{
    // Erro de mapeamento ou conexão
    _logger.LogError(ex, "Mapping error: {Message}", ex.Message);
    throw;
}
```

## 🚀 Dicas de Performance

### 1. Connection Pooling
```csharp
// Connection string com pooling configurado
"Server=.;Database=MyDb;Trusted_Connection=true;Max Pool Size=100;Min Pool Size=5;"
```

### 2. Compiled Queries
Para queries frequentes, considere cache de execution plans.

### 3. Bulk Operations
```csharp
// Para inserções em massa
public async Task BulkInsertAsync(IEnumerable<Product> products)
{
    var sql = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)";
    await _connection.ExecuteAsync(sql, products);
}
```

## 📚 Recursos Adicionais

- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [Dapper Tutorial](https://dapper-tutorial.net/)
- [Performance Comparison](https://github.com/DapperLib/Dapper#performance)
