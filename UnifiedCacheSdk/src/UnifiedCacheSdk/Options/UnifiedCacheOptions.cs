namespace UnifiedCacheSdk.Options;

public sealed class UnifiedCacheOptions
{
    public string ServiceId { get; set; } = string.Empty;
    public string GlobalPrefix { get; set; } = "cache";
    public string Separator { get; set; } = ":";
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromMinutes(5);
}
