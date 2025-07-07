# Simple Web API

## 📚 Conceitos Abordados

Este projeto demonstra a criação de uma Web API simples em ASP.NET Core:

- **ASP.NET Core Web API**: Framework para APIs REST
- **Controllers**: Controladores MVC para endpoints
- **Action Methods**: Métodos que respondem a requisições HTTP
- **HTTP Verbs**: GET, POST, PUT, DELETE
- **Model Binding**: Vinculação automática de dados
- **Content Negotiation**: Negociação de formato de resposta
- **Status Codes**: Códigos de resposta HTTP apropriados

## 🎯 Objetivos de Aprendizado

- Criar endpoints RESTful
- Implementar operações CRUD básicas
- Configurar roteamento de API
- Trabalhar com diferentes formatos de dados
- Implementar validação de entrada
- Retornar respostas HTTP apropriadas

## 💡 Conceitos Importantes

### Basic Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Laptop", Price = 999.99m },
        new Product { Id = 2, Name = "Mouse", Price = 29.99m }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        return Ok(_products);
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> Create(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
        
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
```

### Model Classes
```csharp
public class Product
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## 🚀 Como Executar

```bash
cd SimpleWebAPI
dotnet run
```

Acesse: `https://localhost:5001/api/products`

## 📖 O que Você Aprenderá

1. **HTTP Methods**:
   - GET: Recuperar recursos
   - POST: Criar novos recursos
   - PUT: Atualizar recursos completos
   - PATCH: Atualizar recursos parcialmente
   - DELETE: Remover recursos

2. **Status Codes**:
   - 200 OK: Sucesso
   - 201 Created: Recurso criado
   - 400 Bad Request: Dados inválidos
   - 404 Not Found: Recurso não encontrado
   - 500 Internal Server Error: Erro no servidor

3. **Content Types**:
   - application/json (padrão)
   - application/xml
   - text/plain

4. **Routing**:
   - Attribute routing
   - Convention-based routing
   - Route parameters
   - Query strings

## 🎨 Implementações Completas

### 1. CRUD Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static List<Product> _products = new List<Product>();
    private static int _nextId = 1;

    // GET api/products
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll(
        [FromQuery] string search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        var query = _products.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        return Ok(query.ToList());
    }

    // GET api/products/5
    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { Message = $"Product with ID {id} not found" });

        return Ok(product);
    }

    // POST api/products
    [HttpPost]
    public ActionResult<Product> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = new Product
        {
            Id = _nextId++,
            Name = dto.Name,
            Price = dto.Price,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _products.Add(product);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT api/products/5
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateProductDto dto)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Description = dto.Description;

        return NoContent();
    }

    // DELETE api/products/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound();

        _products.Remove(product);
        return NoContent();
    }
}
```

### 2. DTOs (Data Transfer Objects)
```csharp
public class CreateProductDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }
}

public class UpdateProductDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }
}
```

### 3. Error Handling
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Message = "An error occurred while processing your request",
            Details = exception.Message
        };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => 400,
            KeyNotFoundException => 404,
            UnauthorizedAccessException => 401,
            _ => 500
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

### 4. Custom Validation
```csharp
public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime date)
        {
            return date > DateTime.Now;
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a future date";
    }
}

public class ProductDto
{
    [Required]
    public string Name { get; set; }

    [FutureDate(ErrorMessage = "Launch date must be in the future")]
    public DateTime LaunchDate { get; set; }
}
```

## 🏗️ Configuration

### Program.cs Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelStateFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Custom Filters
```csharp
public class ValidateModelStateFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(new
            {
                Message = "Validation failed",
                Errors = errors
            });
        }
    }
}
```

## 🔍 Pontos de Atenção

### Validation
```csharp
// ✅ Always validate input
[HttpPost]
public ActionResult<Product> Create([FromBody] CreateProductDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // Process valid data
}
```

### Status Codes
```csharp
// ✅ Use appropriate status codes
[HttpGet("{id}")]
public ActionResult<Product> GetById(int id)
{
    var product = _products.FirstOrDefault(p => p.Id == id);
    
    if (product == null)
        return NotFound(); // 404
    
    return Ok(product); // 200
}

[HttpPost]
public ActionResult<Product> Create(Product product)
{
    _products.Add(product);
    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product); // 201
}
```

### Security
```csharp
// ✅ Validate and sanitize input
public class SanitizeInputAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is string str)
            {
                // Sanitize string input
                var sanitized = HttpUtility.HtmlEncode(str);
                // Update the argument
            }
        }
    }
}
```

## 🚀 Testing

### Unit Testing
```csharp
[Test]
public void GetById_WithValidId_ReturnsProduct()
{
    // Arrange
    var controller = new ProductsController();
    
    // Act
    var result = controller.GetById(1);
    
    // Assert
    Assert.IsInstanceOf<OkObjectResult>(result.Result);
    var okResult = result.Result as OkObjectResult;
    Assert.IsInstanceOf<Product>(okResult.Value);
}
```

### Integration Testing
```csharp
[Test]
public async Task GetProducts_ReturnsSuccessStatusCode()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/products");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    Assert.IsNotNull(content);
}
```

## 📚 Recursos Adicionais

- [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [REST API Best Practices](https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [HTTP Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
