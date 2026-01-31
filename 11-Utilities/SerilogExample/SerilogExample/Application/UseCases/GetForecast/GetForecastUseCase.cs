using SerilogExample.Domain.Entities;
using SerilogExample.Domain.Ports;

namespace SerilogExample.Application.UseCases.GetForecast;

public class GetForecastUseCase : IGetForecast
{
    private readonly IWeatherProvider _weatherProvider;

    public GetForecastUseCase(IWeatherProvider weatherProvider)
    {
        _weatherProvider = weatherProvider;
    }

    public async Task<IReadOnlyCollection<WeatherReading>> ExecuteAsync(int days, CancellationToken cancellationToken = default)
    {
        int requestedDays = Math.Clamp(days, 1, 14);
        return await _weatherProvider.GetForecastAsync(requestedDays, cancellationToken);
    }
}
