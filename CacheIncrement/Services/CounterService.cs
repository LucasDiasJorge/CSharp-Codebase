using CacheIncrement.Data;
using CacheIncrement.Interfaces;
using CacheIncrement.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CacheIncrement.Services
{
    public class CounterService : ICounterService
    {
        private readonly IDatabase _redis;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CounterService> _logger;

        public CounterService(
            IConnectionMultiplexer redis, 
            ApplicationDbContext dbContext, 
            ILogger<CounterService> logger)
        {
            _redis = redis.GetDatabase();
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CounterResponse> IncrementAsync(string counterId, long incrementBy = 1)
        {
            try
            {
                var redisKey = GetRedisKey(counterId);
                var newValue = await _redis.StringIncrementAsync(redisKey, incrementBy);
                
                _logger.LogInformation("Incremented counter {CounterId} by {IncrementBy}. New value: {NewValue}", 
                    counterId, incrementBy, newValue);

                return new CounterResponse
                {
                    CounterId = counterId,
                    Count = newValue,
                    Source = "Redis"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter {CounterId}", counterId);
                throw;
            }
        }

        public async Task<CounterResponse> GetCounterAsync(string counterId, bool forceFromDatabase = false)
        {
            try
            {
                if (forceFromDatabase)
                {
                    var dbCounter = await _dbContext.Counters.FindAsync(counterId);
                    return new CounterResponse
                    {
                        CounterId = counterId,
                        Count = dbCounter?.Value ?? 0,
                        Source = "MySQL",
                        Timestamp = dbCounter?.LastUpdated ?? DateTime.UtcNow
                    };
                }

                var redisKey = GetRedisKey(counterId);
                var redisValue = await _redis.StringGetAsync(redisKey);
                
                if (redisValue.HasValue)
                {
                    return new CounterResponse
                    {
                        CounterId = counterId,
                        Count = redisValue,
                        Source = "Redis"
                    };
                }

                // If not in Redis, try to get from MySQL and populate Redis
                var counter = await _dbContext.Counters.FindAsync(counterId);
                if (counter != null)
                {
                    await _redis.StringSetAsync(redisKey, counter.Value);
                    return new CounterResponse
                    {
                        CounterId = counterId,
                        Count = counter.Value,
                        Source = "MySQL (loaded to Redis)",
                        Timestamp = counter.LastUpdated
                    };
                }

                // Counter doesn't exist, initialize it
                await _redis.StringSetAsync(redisKey, 0);
                return new CounterResponse
                {
                    CounterId = counterId,
                    Count = 0,
                    Source = "Redis (initialized)"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counter {CounterId}", counterId);
                throw;
            }
        }

        public async Task<CounterResponse> SetCounterAsync(string counterId, long value)
        {
            try
            {
                var redisKey = GetRedisKey(counterId);
                await _redis.StringSetAsync(redisKey, value);
                
                _logger.LogInformation("Set counter {CounterId} to value: {Value}", counterId, value);

                return new CounterResponse
                {
                    CounterId = counterId,
                    Count = value,
                    Source = "Redis"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting counter {CounterId} to {Value}", counterId, value);
                throw;
            }
        }

        public async Task<CounterSyncStatus> GetSyncStatusAsync(string counterId)
        {
            try
            {
                var redisKey = GetRedisKey(counterId);
                var redisValue = await _redis.StringGetAsync(redisKey);
                var dbCounter = await _dbContext.Counters.FindAsync(counterId);

                return new CounterSyncStatus
                {
                    CounterId = counterId,
                    RedisValue = redisValue.HasValue ? redisValue : 0,
                    MySqlValue = dbCounter?.Value ?? 0,
                    LastSyncTime = dbCounter?.LastUpdated ?? DateTime.MinValue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status for counter {CounterId}", counterId);
                throw;
            }
        }

        public async Task<bool> SyncToMySqlAsync(string counterId)
        {
            try
            {
                var redisKey = GetRedisKey(counterId);
                var redisValue = await _redis.StringGetAsync(redisKey);

                if (!redisValue.HasValue)
                {
                    _logger.LogWarning("Counter {CounterId} not found in Redis for sync", counterId);
                    return false;
                }

                var counter = await _dbContext.Counters.FindAsync(counterId);
                if (counter == null)
                {
                    counter = new Counter
                    {
                        Id = counterId,
                        Value = redisValue,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };
                    _dbContext.Counters.Add(counter);
                }
                else
                {
                    counter.Value = redisValue;
                    counter.LastUpdated = DateTime.UtcNow;
                    _dbContext.Counters.Update(counter);
                }

                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Successfully synced counter {CounterId} with value {Value} to MySQL", 
                    counterId, (long)redisValue);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing counter {CounterId} to MySQL", counterId);
                return false;
            }
        }

        public async Task<List<Counter>> GetAllCountersFromMySqlAsync()
        {
            try
            {
                return await _dbContext.Counters
                    .OrderBy(c => c.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all counters from MySQL");
                throw;
            }
        }

        private static string GetRedisKey(string counterId)
        {
            return $"counter:{counterId}";
        }
    }
}
