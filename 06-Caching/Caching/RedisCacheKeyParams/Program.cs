using RedisCacheKeyParams.CacheKeys;

namespace RedisCacheKeyParams;

public static class Program
{
	public static void Main()
	{
		DefaultRedisCacheKeys cacheKeys = new DefaultRedisCacheKeys();
		DefaultRedisCacheKeys slashCacheKeys = new DefaultRedisCacheKeys("/");

		Console.WriteLine();
		Console.WriteLine("=== Composicao com params object[] ===");
		Console.WriteLine(cacheKeys.Join("cache", "user", 42));
		Console.WriteLine(cacheKeys.Join("cache", "status", "active"));
		Console.WriteLine(cacheKeys.Join(cacheKeys.Namespace, "batch", 7, "v2"));
		Console.WriteLine(cacheKeys.Join(cacheKeys.Namespace, "tenant", 12, "summary", DateOnly.FromDateTime(DateTime.UtcNow)));
		Console.WriteLine(slashCacheKeys.JoinWith(":", slashCacheKeys.Namespace, "status", "cached"));
	}
}
