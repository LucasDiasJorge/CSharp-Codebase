using LoadBalancingAlgorithms.Models;

namespace LoadBalancingAlgorithms.Algorithms;

public class LeastConnectionsLoadBalancer : ILoadBalancer
{
    public string Name => "Least Connections";

    public Server SelectServer(Request request, IReadOnlyList<Server> servers)
    {
        if (servers.Count == 0) throw new InvalidOperationException("No servers available");
        return servers.MinBy(s => s.ActiveConnections)!;
    }
}
