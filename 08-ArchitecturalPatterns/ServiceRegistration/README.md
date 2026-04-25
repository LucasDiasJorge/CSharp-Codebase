# Dependency Injection e Service Registration

## Visão geral

Projeto didático do CSharp-101 dedicado a Dependency Injection e Service Registration, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

Este projeto demonstra os fundamentos de Dependency Injection em ASP.NET Core:

- **Dependency Injection**: Padrão de inversão de controle
- **Service Lifetimes**: Singleton, Scoped, Transient
- **Service Registration**: Registro de serviços no container
- **Service Resolution**: Resolução automática de dependências
- **Interface Segregation**: Uso de interfaces para desacoplamento
- **Middleware Integration**: Injeção em middlewares
- **Constructor Injection**: Injeção via construtor

## Objetivos de aprendizagem

- Entender os diferentes lifetimes de serviços
- Configurar o container de DI adequadamente
- Implementar interfaces para desacoplamento
- Gerenciar dependências complexas
- Otimizar performance através de lifetimes apropriados
- Integrar DI com middlewares e endpoints

### O que Você Aprenderá

1. **Service Lifetimes Detalhados**:
   - **Singleton**: Compartilhado globalmente, ideal para serviços stateless
   - **Scoped**: Por requisição, ideal para contextos de banco de dados
   - **Transient**: Sempre nova instância, ideal para serviços leves

2. **Quando Usar Cada Lifetime**:
   - Configuration services → Singleton
   - DbContext → Scoped  
   - Repository/Business services → Scoped
   - Utility services → Transient

3. **Dependency Resolution**:
   - Constructor injection
   - Service locator pattern
   - Factory pattern com DI

4. **Best Practices**:
   - Evitar service locator anti-pattern
   - Preferir constructor injection
   - Gerenciar memory leaks

## Estrutura do projeto

```text
ServiceRegistration/
+-- Properties/
|   \-- launchSettings.json
+-- Services/
|   +-- Interface/
|   +-- MyService.cs
|   +-- PostExecutionService.cs
|   \-- PreExecutionService.cs
+-- appsettings.Development.json
+-- appsettings.json
+-- Program.cs
+-- ServiceRegistration.csproj
\-- ServiceRegistration.csproj.user
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/ServiceRegistration/ServiceRegistration.csproj
```

Acesse `http://localhost:5000/` e observe os logs no console para entender os diferentes comportamentos dos lifetimes.

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Singleton

```csharp
// Uma única instância para toda a aplicação
builder.Services.AddSingleton<IMyService, MyService>();
```

##### Scoped

```csharp
// Uma instância por requisição HTTP
builder.Services.AddScoped<IMyService, MyService>();
```

##### Transient

```csharp
// Nova instância a cada resolução
builder.Services.AddTransient<IMyService, MyService>();
```

##### Interface e Implementação

```csharp
public interface IMyService
{
    void LogCreation(string message);
}

public class MyService : IMyService
{
    private readonly int _serviceId;

    public MyService()
    {
        _serviceId = new Random().Next(100000, 999999);
    }

    public void LogCreation(string message)
    {
        Console.WriteLine($"{message} - Service ID: {_serviceId}");
    }
}
```

##### 1. Constructor Injection (Recomendado)

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }
}
```

##### 2. Multiple Implementations

```csharp
public interface INotificationService
{
    Task SendAsync(string message);
}

public class EmailNotificationService : INotificationService
{
    public async Task SendAsync(string message)
    {
        // Email implementation
    }
}

public class SmsNotificationService : INotificationService
{
    public async Task SendAsync(string message)
    {
        // SMS implementation
    }
}

// Registration
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<SmsNotificationService>();
builder.Services.AddScoped<INotificationService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var notificationType = config["NotificationType"];
    
    return notificationType switch
    {
        "Email" => provider.GetRequiredService<EmailNotificationService>(),
        "SMS" => provider.GetRequiredService<SmsNotificationService>(),
        _ => provider.GetRequiredService<EmailNotificationService>()
    };
});
```

##### 3. Factory Pattern

```csharp
public interface IServiceFactory<T>
{
    T Create(string type);
}

public class NotificationServiceFactory : IServiceFactory<INotificationService>
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public INotificationService Create(string type)
    {
        return type.ToLower() switch
        {
            "email" => _serviceProvider.GetRequiredService<EmailNotificationService>(),
            "sms" => _serviceProvider.GetRequiredService<SmsNotificationService>(),
            _ => throw new ArgumentException($"Unknown notification type: {type}")
        };
    }
}

// Registration
builder.Services.AddScoped<IServiceFactory<INotificationService>, NotificationServiceFactory>();
```

##### 4. Conditional Registration

```csharp
public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Conditional registration based on environment
        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddScoped<IProductRepository, InMemoryProductRepository>();
        }
        else
        {
            services.AddScoped<IProductRepository, SqlProductRepository>();
        }

        // Always register
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}

// Usage
builder.Services.AddBusinessServices(builder.Configuration);
```

##### 1. Decorators

```csharp
public class LoggingProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly ILogger<LoggingProductService> _logger;

    public LoggingProductService(IProductService inner, ILogger<LoggingProductService> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);
        var product = await _inner.GetByIdAsync(id);
        _logger.LogInformation("Retrieved product: {ProductName}", product?.Name);
        return product;
    }
}

// Registration using Scrutor for decoration
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.Decorate<IProductService, LoggingProductService>();
```

##### 2. Generic Services

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;

    public Repository(DbContext context)
    {
        _context = context;
    }

    // Implementation...
}

// Registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

##### 3. Configuration-based Registration

```csharp
public class ServiceConfiguration
{
    public Dictionary<string, string> Services { get; set; } = new();
}

public static class ConfigurationServiceExtensions
{
    public static IServiceCollection AddConfiguredServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceConfig = configuration.GetSection("Services").Get<ServiceConfiguration>();
        
        foreach (var service in serviceConfig.Services)
        {
            var interfaceType = Type.GetType(service.Key);
            var implementationType = Type.GetType(service.Value);
            
            if (interfaceType != null && implementationType != null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }

        return services;
    }
}
```

##### Memory Leaks

```csharp
// ❌ Problemático - Singleton dependendo de Scoped
builder.Services.AddSingleton<ISingletonService, SingletonService>(); // Não deve depender de Scoped
builder.Services.AddScoped<IScopedService, ScopedService>();

// ✅ Correto - Use Factory para resolver dependências dinâmicas
builder.Services.AddSingleton<ISingletonService>(provider =>
    new SingletonService(() => provider.CreateScope().ServiceProvider.GetRequiredService<IScopedService>()));
```

##### Performance

```csharp
// ✅ Cache expensive computations em Singletons
builder.Services.AddSingleton<IExpensiveService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new ExpensiveService(config); // Computed once
});

// ✅ Use Scoped para serviços que mantêm state por request
builder.Services.AddScoped<IUserContext, UserContext>();
```

##### Thread Safety

```csharp
// ⚠️ Singletons devem ser thread-safe
public class ThreadSafeSingletonService
{
    private readonly ConcurrentDictionary<string, object> _cache = new();
    
    public object GetOrAdd(string key, Func<object> factory)
    {
        return _cache.GetOrAdd(key, _ => factory());
    }
}
```

##### Unit Testing

```csharp
[Test]
public async Task GetProduct_WithValidId_ReturnsProduct()
{
    // Arrange
    var mockRepository = new Mock<IProductRepository>();
    var mockLogger = new Mock<ILogger<ProductService>>();
    
    mockRepository.Setup(r => r.GetByIdAsync(1))
              .ReturnsAsync(new Product { Id = 1, Name = "Test" });
    
    var service = new ProductService(mockRepository.Object, mockLogger.Object);
    
    // Act
    var result = await service.GetByIdAsync(1);
    
    // Assert
    Assert.That(result.Name, Is.EqualTo("Test"));
}
```

##### Integration Testing

```csharp
public class TestStartup : Startup
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        
        // Replace services for testing
        services.Replace(ServiceDescriptor.Scoped<IProductRepository, MockProductRepository>());
    }
}
```

## Referências

- [Dependency Injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Scrutor - Assembly Scanning](https://github.com/khellang/Scrutor)
