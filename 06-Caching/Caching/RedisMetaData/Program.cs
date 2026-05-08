using RedisMetaData.Models;
using RedisMetaData.Services;

RedisConfig config = new()
{
    Host = "localhost",
    Port = 6379,
    Database = 0
};

using RedisCache cache = new(config);

cache.SetEntry("user:1", "name", "Alice", TimeSpan.FromMinutes(10));
string? name = cache.GetEntry<string>("user:1", "name");

Console.WriteLine($"User name: {name}");
