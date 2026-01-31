using LoadBalancingAlgorithms.Models;

namespace LoadBalancingAlgorithms.Algorithms;

public class RoundRobinLoadBalancer : ILoadBalancer
{
    private int _index = -1;
    private readonly object _lock = new();
    public string Name => "Round-Robin";

    public Server SelectServer(Request request, IReadOnlyList<Server> servers)
    {
        if (servers.Count == 0) throw new InvalidOperationException("No servers available");
        lock (_lock)
        {
            _index = (_index + 1) % servers.Count;
            return servers[_index];
        }
    }
}
