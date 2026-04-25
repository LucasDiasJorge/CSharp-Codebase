using Microsoft.AspNetCore.Mvc;
using SerilogExample.Domain.Entities;
using SerilogExample.Domain.Ports;

namespace SerilogExample.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly IGetForecast _getForecast;
    private readonly ILogger<ForecastController> _logger;

    public ForecastController(IGetForecast getForecast, ILogger<ForecastController> logger)
    {
        _getForecast = getForecast;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? days, CancellationToken ct)
    {
        int daysToFetch = days ?? 5;
        try
        {
            _logger.LogInformation("ForecastController.Get called with {Days}", daysToFetch);
            IReadOnlyCollection<WeatherReading> forecast = await _getForecast.ExecuteAsync(daysToFetch, ct);
            _logger.LogInformation("ForecastController.Get returning {Count} readings", forecast.Count);
            return Ok(forecast);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ForecastController.Get cancelled for {Days}", daysToFetch);
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ForecastController.Get failed for {Days}", daysToFetch);
            return Problem("An error occurred while getting the forecast", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
