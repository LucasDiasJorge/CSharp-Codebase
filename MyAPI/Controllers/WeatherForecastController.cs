using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPI.Data;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [ApiController]
    [Route("/api/weather")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(AppDbContext context, ILogger<WeatherForecastController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Endpoint para listar todas as entidades
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetWeatherForecasts()
        {
            try
            {
                // Obtém todos os registros da tabela WeatherForecasts
                var forecasts = await _context.WeatherForecasts.ToListAsync();
                return Ok(forecasts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar os registros");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // Endpoint para verificar a conectividade
        [HttpGet("ping")]
        public async Task<string?> Ping()
        {
            HttpClient httpClient = new();

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("https://example.com");

            return await httpResponseMessage.Content.ReadAsStringAsync();
        }
    }
}
