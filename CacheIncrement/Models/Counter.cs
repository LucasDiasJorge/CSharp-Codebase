using System.ComponentModel.DataAnnotations;

namespace CacheIncrement.Models
{
    public class Counter
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        
        public long Value { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
