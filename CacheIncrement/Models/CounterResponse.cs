namespace CacheIncrement.Models
{
    public class CounterResponse
    {
        public string CounterId { get; set; } = string.Empty;
        public long Count { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = "Redis"; // Redis or MySQL
    }
    
    public class CounterSyncStatus
    {
        public string CounterId { get; set; } = string.Empty;
        public long RedisValue { get; set; }
        public long MySqlValue { get; set; }
        public DateTime LastSyncTime { get; set; }
        public bool IsSynced => RedisValue == MySqlValue;
    }
}
