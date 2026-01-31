using CacheIncrement.Data;
using CacheIncrement.Interfaces;
using CacheIncrement.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Cache Increment API", 
        Version = "v1",
        Description = "Demonstrates Redis for fast increments with periodic MySQL persistence"
    });
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Redis configuration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString!);
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

// Register services
builder.Services.AddScoped<ICounterService, CounterService>();
builder.Services.AddHostedService<CounterSyncService>();

// CORS configuration (optional, for frontend integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging configuration
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cache Increment API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Console.WriteLine("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
        Console.WriteLine("Please make sure MySQL is running and connection string is correct.");
    }
}

// Test Redis connection
using (var scope = app.Services.CreateScope())
{
    try
    {
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var db = redis.GetDatabase();
        await db.StringSetAsync("test_connection", "OK");
        var result = await db.StringGetAsync("test_connection");
        await db.KeyDeleteAsync("test_connection");
        
        if (result == "OK")
        {
            Console.WriteLine("Redis connection successful.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error connecting to Redis: {ex.Message}");
        Console.WriteLine("Please make sure Redis is running and connection string is correct.");
    }
}

Console.WriteLine("Cache Increment API is starting...");
Console.WriteLine("Available endpoints:");
Console.WriteLine("- POST /api/counter/{counterId}/increment");
Console.WriteLine("- GET /api/counter/{counterId}");
Console.WriteLine("- PUT /api/counter/{counterId}");
Console.WriteLine("- GET /api/counter/{counterId}/sync-status");
Console.WriteLine("- POST /api/counter/{counterId}/sync");
Console.WriteLine("- GET /api/counter/mysql/all");
Console.WriteLine("- GET /api/counter/health");

app.Run();
