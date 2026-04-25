using CacheIncrement.Interfaces;
using CacheIncrement.Models;
using Microsoft.AspNetCore.Mvc;

namespace CacheIncrement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CounterController : ControllerBase
    {
        private readonly ICounterService _counterService;
        private readonly ILogger<CounterController> _logger;

        public CounterController(ICounterService counterService, ILogger<CounterController> logger)
        {
            _counterService = counterService;
            _logger = logger;
        }

        /// <summary>
        /// Increment a counter by 1 or specified amount
        /// </summary>
        [HttpPost("{counterId}/increment")]
        public async Task<ActionResult<CounterResponse>> Increment(
            string counterId, 
            [FromQuery] long incrementBy = 1)
        {
            try
            {
                var result = await _counterService.IncrementAsync(counterId, incrementBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter {CounterId}", counterId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get current counter value
        /// </summary>
        [HttpGet("{counterId}")]
        public async Task<ActionResult<CounterResponse>> GetCounter(
            string counterId, 
            [FromQuery] bool forceFromDatabase = false)
        {
            try
            {
                var result = await _counterService.GetCounterAsync(counterId, forceFromDatabase);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counter {CounterId}", counterId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Set counter to specific value
        /// </summary>
        [HttpPut("{counterId}")]
        public async Task<ActionResult<CounterResponse>> SetCounter(
            string counterId, 
            [FromBody] SetCounterRequest request)
        {
            try
            {
                var result = await _counterService.SetCounterAsync(counterId, request.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting counter {CounterId}", counterId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get synchronization status between Redis and MySQL
        /// </summary>
        [HttpGet("{counterId}/sync-status")]
        public async Task<ActionResult<CounterSyncStatus>> GetSyncStatus(string counterId)
        {
            try
            {
                var result = await _counterService.GetSyncStatusAsync(counterId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status for counter {CounterId}", counterId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Manually trigger sync for specific counter to MySQL
        /// </summary>
        [HttpPost("{counterId}/sync")]
        public async Task<ActionResult> SyncToMySQL(string counterId)
        {
            try
            {
                var success = await _counterService.SyncToMySqlAsync(counterId);
                if (success)
                {
                    return Ok(new { message = "Counter synced successfully", counterId });
                }
                else
                {
                    return BadRequest(new { message = "Failed to sync counter", counterId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing counter {CounterId}", counterId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all counters from MySQL database
        /// </summary>
        [HttpGet("mysql/all")]
        public async Task<ActionResult<List<Counter>>> GetAllCountersFromMySQL()
        {
            try
            {
                var result = await _counterService.GetAllCountersFromMySqlAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all counters from MySQL");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public ActionResult Health()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                service = "CacheIncrement API"
            });
        }
    }

    public class SetCounterRequest
    {
        public long Value { get; set; }
    }
}
