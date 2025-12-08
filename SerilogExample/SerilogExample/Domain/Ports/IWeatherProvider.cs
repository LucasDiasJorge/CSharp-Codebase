using SerilogExample.Domain.Entities;

namespace SerilogExample.Domain.Ports;

public interface IWeatherProvider
{
    Task<IReadOnlyCollection<WeatherReading>> GetForecastAsync(int days, CancellationToken cancellationToken = default);
}
