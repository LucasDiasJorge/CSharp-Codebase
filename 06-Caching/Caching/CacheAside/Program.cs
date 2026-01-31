using Microsoft.EntityFrameworkCore;
using CacheAside.Data;
using CacheAside.Interfaces;
using CacheAside.Repositories;
using CacheAside.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework with In-Memory Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("CacheAsideDb"));

// Add Memory Cache
builder.Services.AddMemoryCache();

// Register services
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Cache Aside Pattern API", 
        Version = "v1",
        Description = "A demonstration of the Cache Aside pattern in .NET 8" 
    });
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cache Aside Pattern API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add some logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopwatch.Stop();
    
    logger.LogInformation("Response: {StatusCode} in {ElapsedMilliseconds}ms", 
        context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
});

app.Run();
