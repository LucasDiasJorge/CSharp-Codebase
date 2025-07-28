using Microsoft.Extensions.Caching.Memory;
using CacheAside.Interfaces;
using System.Text.Json;

namespace CacheAside.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    _logger.LogInformation("Cache hit for key: {Key}", key);
                    return Task.FromResult(cachedValue as T);
                }

                _logger.LogInformation("Cache miss for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions();
                
                if (expiration.HasValue)
                {
                    cacheOptions.SetAbsoluteExpiration(expiration.Value);
                }
                else
                {
                    // Default expiration of 5 minutes
                    cacheOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                }

                _memoryCache.Set(key, value, cacheOptions);
                _logger.LogInformation("Value cached for key: {Key} with expiration: {Expiration}", 
                    key, expiration ?? TimeSpan.FromMinutes(5));
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogInformation("Removed cache entry for key: {Key}", key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            // Note: MemoryCache doesn't have built-in pattern removal
            // This is a simplified implementation
            _logger.LogWarning("Pattern-based removal not fully supported in MemoryCache for pattern: {Pattern}", pattern);
            return Task.CompletedTask;
        }
    }
}
