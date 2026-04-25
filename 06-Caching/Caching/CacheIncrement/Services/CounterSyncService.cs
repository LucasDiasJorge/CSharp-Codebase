using CacheIncrement.Data;
using CacheIncrement.Models;
using StackExchange.Redis;

namespace CacheIncrement.Services
{
    public class CounterSyncService : BackgroundService
    {
        private readonly IDatabase _redis;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CounterSyncService> _logger;
        private readonly TimeSpan _syncInterval;

        public CounterSyncService(
            IConnectionMultiplexer redis,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<CounterSyncService> logger)
        {
            _redis = redis.GetDatabase();
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            
            var intervalMinutes = configuration.GetValue<int>("CounterSync:IntervalMinutes", 1);
            _syncInterval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CounterSyncService started. Sync interval: {Interval}", _syncInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncAllCountersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during counter synchronization");
                }

                try
                {
                    await Task.Delay(_syncInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation token is triggered
                    break;
                }
            }

            _logger.LogInformation("CounterSyncService stopped");
        }

        private async Task SyncAllCountersAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Get all Redis keys that start with "counter:"
                var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
                var redisKeys = server.Keys(pattern: "counter:*").ToList();

                _logger.LogInformation("Starting sync for {CounterCount} counters", redisKeys.Count);

                var syncedCount = 0;
                var errorCount = 0;

                foreach (var redisKey in redisKeys)
                {
                    try
                    {
                        var counterId = redisKey.ToString().Replace("counter:", "");
                        var redisValue = await _redis.StringGetAsync(redisKey);

                        if (!redisValue.HasValue)
                            continue;

                        var counter = await dbContext.Counters.FindAsync(counterId);
                        if (counter == null)
                        {
                            counter = new Counter
                            {
                                Id = counterId,
                                Value = (long)redisValue,
                                CreatedAt = DateTime.UtcNow,
                                LastUpdated = DateTime.UtcNow
                            };
                            dbContext.Counters.Add(counter);
                        }
                        else
                        {
                            // Only update if Redis value is different (to avoid unnecessary writes)
                            if (counter.Value != (long)redisValue)
                            {
                                counter.Value = (long)redisValue;
                                counter.LastUpdated = DateTime.UtcNow;
                                dbContext.Counters.Update(counter);
                            }
                        }

                        syncedCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _logger.LogError(ex, "Error syncing individual counter {RedisKey}", redisKey);
                    }
                }

                if (syncedCount > 0)
                {
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Successfully synced {SyncedCount} counters to MySQL. Errors: {ErrorCount}", 
                        syncedCount, errorCount);
                }
                else
                {
                    _logger.LogInformation("No counters to sync");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk counter synchronization");
            }
        }
    }
}
