using Serilog;
using SerilogExample.Application.UseCases.GetForecast;
using SerilogExample.Domain.Ports;
using SerilogExample.Infrastructure.Adapters.Weather;

namespace SerilogExample;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((context, services, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
        );

        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();

        builder.Services.AddControllers();

        builder.Services.AddSingleton<IWeatherProvider, InMemoryWeatherProvider>();
        builder.Services.AddScoped<IGetForecast, GetForecastUseCase>();

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Mapear controllers (ex: ForecastController)
        app.MapControllers();

        try
        {
            Log.Information("Iniciando aplicação web");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Aplicação falhou ao iniciar");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
