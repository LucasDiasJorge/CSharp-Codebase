using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using MyApiClientNamespace;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Build and configure the web application
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        var app = builder.Build();
        
        // Configure Swagger
        app.UseSwagger(options => {});
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
        app.MapControllers();

        // Start the server in the background
        var serverTask = app.RunAsync();  // No need for Task.Run, RunAsync already returns a Task

        // Wait a moment for the server to start
        await Task.Delay(2000);

        try
        {
            // Create and use the API client
            var httpClient = new HttpClient();
            var client = new GeneratedApiClient("http://localhost:5097", httpClient); // Use correct port
            var user = await client.UserAsync(1);
            Console.WriteLine($"Fetched User: {user.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling API: {ex.Message}");
        }

        // Keep the server running
        await serverTask;
    }
}