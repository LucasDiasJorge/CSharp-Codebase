using Microsoft.AspNetCore.Mvc;
using CacheAside.Interfaces;

namespace CacheAside.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Clear a specific cache entry
        /// </summary>
        /// <param name="key">Cache key to clear</param>
        /// <returns>Success status</returns>
        [HttpDelete("{key}")]
        public async Task<ActionResult> ClearCacheEntry(string key)
        {
            _logger.LogInformation("API call: Clearing cache entry for key: {Key}", key);
            
            await _cacheService.RemoveAsync(key);
            return Ok(new { message = $"Cache entry for key '{key}' cleared successfully" });
        }

        /// <summary>
        /// Clear cache entries by pattern
        /// </summary>
        /// <param name="pattern">Cache key pattern to clear</param>
        /// <returns>Success status</returns>
        [HttpDelete("pattern/{pattern}")]
        public async Task<ActionResult> ClearCacheByPattern(string pattern)
        {
            _logger.LogInformation("API call: Clearing cache entries by pattern: {Pattern}", pattern);
            
            await _cacheService.RemoveByPatternAsync(pattern);
            return Ok(new { message = $"Cache entries matching pattern '{pattern}' cleared successfully" });
        }

        /// <summary>
        /// Get cache statistics (for demonstration purposes)
        /// </summary>
        /// <returns>Cache information</returns>
        [HttpGet("stats")]
        public ActionResult GetCacheStats()
        {
            _logger.LogInformation("API call: Getting cache statistics");
            
            return Ok(new
            {
                cacheType = "Memory Cache",
                message = "Cache is running",
                timestamp = DateTime.UtcNow,
                note = "This is a demonstration endpoint. In production, you might want to include actual cache statistics."
            });
        }
    }
}
