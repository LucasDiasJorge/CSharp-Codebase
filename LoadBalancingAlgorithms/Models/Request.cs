namespace LoadBalancingAlgorithms.Models;

public class Request
{
    public required string ClientIp { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
