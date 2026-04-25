using CacheIncrement.Models;

namespace CacheIncrement.Interfaces
{
    public interface ICounterService
    {
        Task<CounterResponse> IncrementAsync(string counterId, long incrementBy = 1);
        Task<CounterResponse> GetCounterAsync(string counterId, bool forceFromDatabase = false);
        Task<CounterResponse> SetCounterAsync(string counterId, long value);
        Task<CounterSyncStatus> GetSyncStatusAsync(string counterId);
        Task<bool> SyncToMySqlAsync(string counterId);
        Task<List<Counter>> GetAllCountersFromMySqlAsync();
    }
}
