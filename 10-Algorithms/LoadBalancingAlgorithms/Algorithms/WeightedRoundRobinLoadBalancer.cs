using LoadBalancingAlgorithms.Models;

namespace LoadBalancingAlgorithms.Algorithms;

public class WeightedRoundRobinLoadBalancer : ILoadBalancer
{
    private int _currentIndex = -1;
    private int _currentWeight = 0;
    private int _maxWeight = 0;
    private int _gcdWeight = 1;
    private IReadOnlyList<Server>? _serversCache;
    public string Name => "Weighted Round-Robin";

    public Server SelectServer(Request request, IReadOnlyList<Server> servers)
    {
        if (servers.Count == 0) throw new InvalidOperationException("No servers available");
        if (_serversCache != servers)
        {
            _serversCache = servers;
            _maxWeight = servers.Max(s => s.Weight);
            _gcdWeight = servers.Select(s => s.Weight).Aggregate(Gcd);
            _currentIndex = -1;
            _currentWeight = 0;
        }
        while (true)
        {
            _currentIndex = (_currentIndex + 1) % servers.Count;
            if (_currentIndex == 0)
            {
                _currentWeight -= _gcdWeight;
                if (_currentWeight <= 0)
                {
                    _currentWeight = _maxWeight;
                    if (_currentWeight == 0)
                        throw new InvalidOperationException("All server weights are zero");
                }
            }
            if (servers[_currentIndex].Weight >= _currentWeight)
                return servers[_currentIndex];
        }
    }

    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}
