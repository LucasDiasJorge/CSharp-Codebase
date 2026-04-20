# Minimal API Demo

## Visão geral

Projeto didático do CSharp-101 dedicado a Minimal API Demo, com foco em ASP.NET Core, contratos HTTP e pipeline web.

## Conceitos abordados

Este projeto demonstra o uso de Minimal APIs em .NET, incluindo:

- **Minimal APIs**: APIs simples sem controllers
- **Endpoint Configuration**: Configuração de rotas e endpoints
- **Dependency Injection**: Injeção de dependências em minimal APIs
- **Entity Framework**: Integração com banco de dados
- **Health Checks**: Monitoramento da saúde da aplicação
- **CORS**: Configuração de Cross-Origin Resource Sharing
- **Actuators**: Endpoints de monitoramento e métricas

## Objetivos de aprendizagem

- Criar APIs simples e eficientes
- Configurar endpoints sem controllers tradicionais
- Integrar com Entity Framework e banco de dados
- Implementar health checks e monitoramento
- Configurar CORS para aplicações client-side
- Usar actuators para observabilidade

### O que Você Aprenderá

1. **Minimal APIs vs Controllers**:
   - Menos boilerplate code
   - Performance superior
   - Ideal para microserviços
   - Sintaxe mais concisa

2. **Route Configuration**:
   - Definição de rotas inline
   - Parameter binding automático
   - Route constraints
   - Route groups

3. **Response Types**:
   - Results.Ok()
   - Results.Created()
   - Results.NotFound()
   - Results.BadRequest()

4. **Middleware Integration**:
   - Como usar middleware com minimal APIs
   - Ordem de execução
   - Custom middleware

## Estrutura do projeto

```text
MinimalApiDemo/
+-- Annotations/
|   \-- ProductPriceAttribute.cs
+-- Models/
|   \-- Product.cs
+-- Properties/
|   \-- launchSettings.json
+-- ApplicationDbContext.cs
+-- appsettings.Development.json
+-- appsettings.json
+-- MinimalApiDemo.csproj
+-- MinimalApiDemo.csproj.user
\-- ...
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/MinimalApiDemo/MinimalApiDemo.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Minimal API Endpoints

```csharp
app.MapGet("/products", (ApplicationDbContext db) => 
    db.Products.ToList());

app.MapPost("/products", (Product product, ApplicationDbContext db) =>
{
    db.Products.Add(product);
    db.SaveChanges();
    return Results.Created($"/products/{product.Id}", product);
});
```

##### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>();

app.MapHealthChecks("/health");
```

##### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

##### Endpoints Disponíveis

- `GET /products` - Listar todos os produtos
- `POST /products` - Criar novo produto
- `GET /products/{id}` - Obter produto por ID
- `PUT /products/{id}` - Atualizar produto
- `DELETE /products/{id}` - Deletar produto
- `GET /health` - Health check da aplicação
- `GET /actuator/*` - Endpoints de monitoramento

##### 1. CRUD Operations

```csharp
var group = app.MapGroup("/api/products");

group.MapGet("/", GetAllProducts);
group.MapGet("/{id}", GetProductById);
group.MapPost("/", CreateProduct);
group.MapPut("/{id}", UpdateProduct);
group.MapDelete("/{id}", DeleteProduct);

static async Task<IResult> GetAllProducts(ApplicationDbContext db)
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
}

static async Task<IResult> GetProductById(int id, ApplicationDbContext db)
{
    var product = await db.Products.FindAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
}
```

##### 2. Validation

```csharp
app.MapPost("/products", async (Product product, ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(product.Name))
        return Results.BadRequest("Name is required");
    
    if (product.Price <= 0)
        return Results.BadRequest("Price must be greater than 0");
    
    db.Products.Add(product);
    await db.SaveChangesAsync();
    
    return Results.Created($"/products/{product.Id}", product);
});
```

##### 3. Route Groups

```csharp
var api = app.MapGroup("/api/v1");

var products = api.MapGroup("/products");
products.MapGet("/", GetProducts);
products.MapPost("/", CreateProduct);

var users = api.MapGroup("/users");
users.MapGet("/", GetUsers);
users.MapPost("/", CreateUser);
```

##### 4. Authentication

```csharp
var secured = app.MapGroup("/api/secure")
                .RequireAuthorization();

secured.MapGet("/profile", (ClaimsPrincipal user) =>
{
    return Results.Ok(new { user.Identity.Name });
});
```

##### 1. OpenAPI/Swagger

```csharp
app.MapPost("/products", CreateProduct)
   .WithName("CreateProduct")
   .WithTags("Products")
   .Produces<Product>(201)
   .ProducesProblem(400);
```

##### 2. Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

##### 3. Response Caching

```csharp
app.MapGet("/products", GetProducts)
   .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));
```

##### Performance

```csharp
// ✅ Use async/await para operações I/O
app.MapGet("/products", async (ApplicationDbContext db) =>
    await db.Products.ToListAsync());

// ✅ Configure connection pooling
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, 
        sqlOptions => sqlOptions.EnableRetryOnFailure()));
```

##### Error Handling

```csharp
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = "An error occurred",
            details = exceptionHandlerPathFeature?.Error?.Message
        }));
    });
});
```

##### Security

```csharp
// HTTPS redirection
app.UseHttpsRedirection();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

##### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://external-api.com"), "External API");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

##### Metrics

```csharp
builder.Services.AddSingleton<IMetrics, Metrics>();

app.MapGet("/metrics", (IMetrics metrics) =>
{
    return Results.Ok(metrics.GetSnapshot());
});
```

## Referências

- [Minimal APIs in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Health checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Steeltoe Actuators](https://steeltoe.io/application-management)
