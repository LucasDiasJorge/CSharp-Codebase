# Cache Aside Pattern Implementation in C#

This project demonstrates the **Cache Aside** pattern (also known as **Lazy Loading**) implementation using C# and .NET 8. The Cache Aside pattern is a caching strategy where the application is responsible for loading data into the cache when needed.

## 🎯 What is the Cache Aside Pattern?

The Cache Aside pattern is a caching strategy where:

1. **Cache Miss**: When data is requested, the application first checks the cache
2. **Load from Source**: If not found in cache, data is loaded from the primary data source (database)
3. **Cache Population**: The loaded data is then stored in the cache for future requests
4. **Cache Hit**: Subsequent requests for the same data are served directly from the cache

### Key Characteristics:
- **Application Managed**: The application logic handles cache operations
- **Lazy Loading**: Data is cached only when requested
- **Cache Miss Penalty**: First request suffers from cache miss latency
- **Consistency Control**: Application controls when to invalidate cache entries

## 🏗️ Project Structure

```
CacheAside/
├── Controllers/           # API Controllers
│   ├── ProductsController.cs
│   └── CacheController.cs
├── Data/                 # Database Context
│   └── ApplicationDbContext.cs
├── Interfaces/           # Service Contracts
│   ├── ICacheService.cs
│   ├── IProductRepository.cs
│   └── IProductService.cs
├── Models/               # Data Models
│   └── Product.cs
├── Repositories/         # Data Access Layer
│   └── ProductRepository.cs
├── Services/             # Business Logic & Cache Implementation
│   ├── MemoryCacheService.cs
│   └── ProductService.cs
├── Program.cs            # Application Configuration
├── appsettings.json      # Configuration Files
└── README.md            # This file
```

## 🚀 Features

### Core Implementation
- **Memory Cache**: Using `IMemoryCache` for demonstration
- **Product Management**: CRUD operations with caching
- **Cache Invalidation**: Automatic cache invalidation on data changes
- **Logging**: Comprehensive logging to track cache hits/misses
- **API Documentation**: Swagger UI for testing

### Cache Strategies Implemented
1. **Read-Through Cache**: Get operations check cache first
2. **Write-Around Cache**: Updates invalidate cache entries
3. **Cache Invalidation**: Strategic removal of stale data

## 🔧 Technologies Used

- **.NET 8**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core**: ORM with In-Memory database
- **Microsoft.Extensions.Caching.Memory**: Built-in memory caching
- **Swagger/OpenAPI**: API documentation and testing
- **Dependency Injection**: Built-in DI container

## 📦 Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Installation

1. **Navigate to the project directory:**
   ```powershell
   cd "C:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\CacheAside"
   ```

2. **Restore NuGet packages:**
   ```powershell
   dotnet restore
   ```

3. **Build the project:**
   ```powershell
   dotnet build
   ```

4. **Run the application:**
   ```powershell
   dotnet run
   ```

5. **Access the API:**
   - Swagger UI: `https://localhost:5001` or `http://localhost:5000`
   - API Base URL: `https://localhost:5001/api` or `http://localhost:5000/api`

## 🧪 Testing the Cache Aside Pattern

### 1. **Test Cache Miss and Cache Hit**

```powershell
# First request (Cache Miss) - Check the logs for "Cache miss"
curl https://localhost:5001/api/products/1

# Second request (Cache Hit) - Check the logs for "Cache hit"
curl https://localhost:5001/api/products/1
```

### 2. **Test Cache Invalidation**

```powershell
# Get a product (loads into cache)
curl https://localhost:5001/api/products/1

# Update the product (invalidates cache)
curl -X PUT https://localhost:5001/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Updated Laptop",
    "description": "Updated description",
    "price": 1099.99,
    "category": "Electronics"
  }'

# Get the product again (cache miss due to invalidation)
curl https://localhost:5001/api/products/1
```

### 3. **Test Different Cache Keys**

```powershell
# Cache different types of requests
curl https://localhost:5001/api/products                    # All products
curl https://localhost:5001/api/products/category/Electronics # By category
curl https://localhost:5001/api/products/1                  # Specific product
```

## 📋 API Endpoints

### Products API
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{category}` - Get products by category
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Cache Management API
- `DELETE /api/cache/{key}` - Clear specific cache entry
- `DELETE /api/cache/pattern/{pattern}` - Clear cache by pattern
- `GET /api/cache/stats` - Get cache statistics

## 🔍 Cache Implementation Details

### Cache Keys Strategy
```csharp
private const string PRODUCT_CACHE_KEY = "product:{0}";           // Individual products
private const string ALL_PRODUCTS_CACHE_KEY = "products:all";     // All products list
private const string CATEGORY_CACHE_KEY = "products:category:{0}"; // Products by category
```

### Cache Expiration
- **Individual Products**: 10 minutes
- **All Products**: 5 minutes  
- **Category Products**: 7 minutes

### Cache Operations Flow

#### Read Operation (Cache Aside):
```csharp
public async Task<Product?> GetProductByIdAsync(int id)
{
    var cacheKey = string.Format(PRODUCT_CACHE_KEY, id);
    
    // 1. Check cache first
    var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey);
    if (cachedProduct != null)
        return cachedProduct; // Cache Hit
    
    // 2. Cache miss - load from database
    var product = await _productRepository.GetByIdAsync(id);
    
    // 3. Store in cache for future requests
    if (product != null)
        await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10));
    
    return product;
}
```

#### Write Operation (Cache Invalidation):
```csharp
public async Task<Product?> UpdateProductAsync(int id, Product product)
{
    // 1. Update in database
    var updatedProduct = await _productRepository.UpdateAsync(id, product);
    
    if (updatedProduct != null)
    {
        // 2. Update cache with fresh data
        var cacheKey = string.Format(PRODUCT_CACHE_KEY, id);
        await _cacheService.SetAsync(cacheKey, updatedProduct, TimeSpan.FromMinutes(10));
        
        // 3. Invalidate related cache entries
        await _cacheService.RemoveAsync(ALL_PRODUCTS_CACHE_KEY);
        await _cacheService.RemoveAsync(string.Format(CATEGORY_CACHE_KEY, product.Category.ToLower()));
    }
    
    return updatedProduct;
}
```

## 📊 Monitoring and Logging

The application includes comprehensive logging to help you understand cache behavior:

- **Cache Hits**: When data is found in cache
- **Cache Misses**: When data is not in cache and loaded from database
- **Cache Invalidation**: When cache entries are removed
- **Database Operations**: When data is fetched from the database
- **API Requests**: Request timing and status codes

### Log Examples:
```
info: CacheAside.Services.MemoryCacheService[0]
      Cache miss for key: product:1

info: CacheAside.Repositories.ProductRepository[0]
      Fetching product from database with ID: 1

info: CacheAside.Services.MemoryCacheService[0]
      Value cached for key: product:1 with expiration: 00:10:00

info: CacheAside.Services.MemoryCacheService[0]
      Cache hit for key: product:1
```

## 🎯 Benefits of This Implementation

1. **Performance**: Reduces database load and improves response times
2. **Scalability**: Cache absorbs read traffic, allowing database to handle writes
3. **Flexibility**: Application controls cache behavior and invalidation
4. **Reliability**: Application continues to work even if cache fails
5. **Cost Efficiency**: Reduces expensive database operations

## ⚡ Best Practices Demonstrated

1. **Proper Cache Key Design**: Hierarchical and predictable cache keys
2. **Cache Invalidation Strategy**: Removes stale data on updates
3. **Error Handling**: Graceful fallback when cache operations fail
4. **Logging and Monitoring**: Comprehensive logging for troubleshooting
5. **Separation of Concerns**: Clear separation between cache, business logic, and data access

## 🔄 Alternative Cache Patterns

This project demonstrates **Cache Aside**, but here are other common patterns:

- **Read-Through**: Cache automatically loads data on miss
- **Write-Through**: All writes go through cache to database
- **Write-Behind**: Writes are batched and written asynchronously
- **Refresh-Ahead**: Cache proactively refreshes before expiration

## 🛠️ Extending the Project

### Adding Redis Cache
Replace `MemoryCacheService` with Redis implementation:

```csharp
// In Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

### Adding Cache Metrics
Implement cache hit/miss ratio tracking:

```csharp
public class CacheMetrics
{
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public double HitRatio => CacheHits + CacheMisses > 0 ? 
        (double)CacheHits / (CacheHits + CacheMisses) : 0;
}
```

### Adding Cache Warming
Pre-load frequently accessed data:

```csharp
public async Task WarmupCacheAsync()
{
    var popularProducts = await _productRepository.GetPopularProductsAsync();
    foreach (var product in popularProducts)
    {
        var cacheKey = string.Format(PRODUCT_CACHE_KEY, product.Id);
        await _cacheService.SetAsync(cacheKey, product);
    }
}
```

## 📝 Sample Data

The application includes pre-seeded sample data:

| ID | Name | Description | Price | Category |
|----|------|-------------|-------|----------|
| 1 | Laptop | High-performance laptop | $999.99 | Electronics |
| 2 | Mouse | Wireless gaming mouse | $79.99 | Electronics |
| 3 | Keyboard | Mechanical keyboard | $129.99 | Electronics |
| 4 | Monitor | 4K gaming monitor | $399.99 | Electronics |
| 5 | Headphones | Noise-cancelling headphones | $249.99 | Electronics |

## 🎓 Learning Outcomes

After exploring this project, you'll understand:

1. **Cache Aside Pattern**: How and when to implement it
2. **Cache Strategy**: Different caching approaches and their trade-offs
3. **Performance Optimization**: How caching improves application performance
4. **Cache Invalidation**: Strategies for maintaining data consistency
5. **Monitoring**: How to track cache effectiveness

## 🤝 Contributing

Feel free to contribute by:
- Adding new cache providers (Redis, Distributed Cache)
- Implementing cache metrics and monitoring
- Adding more sophisticated invalidation strategies
- Creating performance benchmarks
- Improving documentation

## 📚 Further Reading

- [Microsoft Caching Guidance](https://docs.microsoft.com/en-us/azure/architecture/best-practices/caching)
- [Cache Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
- [Redis Cache Patterns](https://redis.io/docs/manual/patterns/)
- [.NET Memory Caching](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory)

---

**Happy Caching! 🚀**
