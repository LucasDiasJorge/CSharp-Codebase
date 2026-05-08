namespace RedisMetaData.Models;

public sealed class RedisConfig
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 6379;

    public int Database { get; init; }

    public string ConnectionString => $"{Host}:{Port}";
}