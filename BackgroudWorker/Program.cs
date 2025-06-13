
namespace BackgroudWorker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IServiceCollection serviceCollection = builder.Services;

        // Add services to the container.
        serviceCollection.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        serviceCollection.AddOpenApi();

        // Hosted service that runs on a timer https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-9.0&tabs=visual-studio
        serviceCollection.AddHostedService<TimedHostedService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapGet("/stoptask", (HttpContext httpContext) =>
        {
            app.Services.GetRequiredService<TimedHostedService>().StopAsync(new CancellationToken());
        });

        app.Run();
    }
}
