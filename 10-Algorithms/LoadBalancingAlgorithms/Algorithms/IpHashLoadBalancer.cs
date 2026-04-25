using System.Security.Cryptography;
using System.Text;
using LoadBalancingAlgorithms.Models;

namespace LoadBalancingAlgorithms.Algorithms;

public class IpHashLoadBalancer : ILoadBalancer
{
    public string Name => "IP Hash";

    public Server SelectServer(Request request, IReadOnlyList<Server> servers)
    {
        if (servers.Count == 0) throw new InvalidOperationException("No servers available");
        // Simple hash using SHA256 of IP, mod number of servers
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(request.ClientIp));
        int hash = BitConverter.ToInt32(bytes, 0);
        if (hash < 0) hash = -hash;
        int index = hash % servers.Count;
        return servers[index];
    }
}
