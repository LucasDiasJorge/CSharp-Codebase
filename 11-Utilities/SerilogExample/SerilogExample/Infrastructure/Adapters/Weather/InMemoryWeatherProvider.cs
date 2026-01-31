using SerilogExample.Domain.Entities;
using SerilogExample.Domain.Ports;

namespace SerilogExample.Infrastructure.Adapters.Weather;

public class InMemoryWeatherProvider : IWeatherProvider
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<IReadOnlyCollection<WeatherReading>> GetForecastAsync(int days, CancellationToken cancellationToken = default)
    {
        WeatherReading[] forecast = Enumerable.Range(1, days).Select(index => new WeatherReading
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        return Task.FromResult<IReadOnlyCollection<WeatherReading>>(forecast);
    }
}
