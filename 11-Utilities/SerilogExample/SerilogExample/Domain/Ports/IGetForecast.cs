using SerilogExample.Domain.Entities;

namespace SerilogExample.Domain.Ports;

public interface IGetForecast
{
    Task<IReadOnlyCollection<WeatherReading>> ExecuteAsync(int days, CancellationToken cancellationToken = default);
}
