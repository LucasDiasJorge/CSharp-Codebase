using KafkaStreamApi.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for better logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Add Serilog

    // Configure Kafka service with validation
    var kafkaConfig = builder.Configuration.GetSection("Kafka");
    var bootstrapServers = kafkaConfig["BootstrapServers"] ?? 
        throw new InvalidOperationException("Kafka BootstrapServers not configured");
    var groupId = kafkaConfig["GroupId"] ?? 
        throw new InvalidOperationException("Kafka GroupId not configured");

    builder.Services.AddSingleton<KafkaConsumerService>(provider => 
        new KafkaConsumerService(
            bootstrapServers,
            groupId,
            provider.GetRequiredService<ILogger<KafkaConsumerService>>()));

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseCors("AllowAll");
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    app.Urls.Add($"http://0.0.0.0:{port}");
    
    Log.Information("Application running on port {Port}", port);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}