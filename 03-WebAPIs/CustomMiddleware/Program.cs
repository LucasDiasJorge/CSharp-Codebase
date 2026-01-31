using CustomMiddleware.Middlewares.Extensions;
using Microsoft.OpenApi.Models;

namespace CustomMiddleware;

public class Program
{
    public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add standard Swagger/OpenAPI instead of AddOpenApi()
        builder.Services.AddEndpointsApiExplorer();

        // Configure middleware options
        builder.Services.Configure<CustomHeaderOptions>(options =>
        {
            options.Headers.Add("X-Powered-By", "My Custom App");
            options.Headers.Add("X-Content-Type-Options", "nosniff");
        });

        var app = builder.Build();
        
        // Use your custom middlewares
        app.UseCustomMiddlewares();
        
        // Standard middleware pipeline
        app.UseDefaultMiddlewares();
        app.UseHttpsRedirection();

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", () =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecast(
                            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            Random.Shared.Next(-20, 90),
                            summaries[Random.Shared.Next(summaries.Length)]
                        ))
                    .ToArray();
                
                // Simulating timout exception
                int sleepMilliseconds = Random.Shared.Next(5000, 20001);
                Thread.Sleep(sleepMilliseconds);
                
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();

        app.Run();
    }
}