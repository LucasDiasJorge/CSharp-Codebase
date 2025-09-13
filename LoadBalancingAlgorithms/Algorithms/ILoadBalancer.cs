using LoadBalancingAlgorithms.Models;

namespace LoadBalancingAlgorithms.Algorithms;

public interface ILoadBalancer
{
    string Name { get; }
    Server SelectServer(Request request, IReadOnlyList<Server> servers);
}
