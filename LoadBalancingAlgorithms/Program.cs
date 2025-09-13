using LoadBalancingAlgorithms.Models;
using LoadBalancingAlgorithms.Algorithms;

Console.WriteLine("Load Balancing Algorithms Simulation\n");

List<Server> baseServers = new()
{
	new Server{ Id = "S1", Weight = 1},
	new Server{ Id = "S2", Weight = 2},
	new Server{ Id = "S3", Weight = 3}
};

List<string> clientIps = new()
{
	"10.0.0.1","10.0.0.2","10.0.0.3","10.0.0.4","10.0.0.5","10.0.0.6","10.0.0.7","10.0.0.8"
};

List<ILoadBalancer> algorithms = new()
{
	new RoundRobinLoadBalancer(),
	new LeastConnectionsLoadBalancer(),
	new WeightedRoundRobinLoadBalancer(),
	new IpHashLoadBalancer()
};

int totalRequests = 200;

foreach (ILoadBalancer algo in algorithms)
{
	Console.WriteLine($"=== {algo.Name} ===");
	// Deep copy servers for isolation
	List<Server> servers = baseServers
		.Select(s => new Server{ Id = s.Id, Weight = s.Weight })
		.ToList();

	Dictionary<string,int> distribution = new();
	Random rng = new(42);

	for (int i = 0; i < totalRequests; i++)
	{
		string ip = clientIps[rng.Next(clientIps.Count)];
		Request req = new() { ClientIp = ip };
		Server chosen = algo.SelectServer(req, servers);
		chosen.HandledRequests++;
		chosen.ActiveConnections++;
		// simulate completion for algorithms needing ActiveConnections dynamics
		if (algo is LeastConnectionsLoadBalancer && chosen.ActiveConnections > 0 && i % 3 == 0)
		{
			chosen.ActiveConnections--; // some finish
		}
		if (!distribution.ContainsKey(chosen.Id)) distribution[chosen.Id] = 0;
		distribution[chosen.Id]++;
	}

	foreach (Server s in servers.OrderBy(s=>s.Id))
	{
		Console.WriteLine($"{s.Id}: requests={s.HandledRequests}, finalActiveConn={s.ActiveConnections}, weight={s.Weight}");
	}
	Console.WriteLine("Distribution: " + string.Join(", ", distribution.OrderBy(k=>k.Key).Select(k => $"{k.Key}={k.Value}")));
	Console.WriteLine();
}

Console.WriteLine("Scenario Recommendations:\n");
Console.WriteLine("E-Commerce (equal servers, stateless): Round-Robin");
Console.WriteLine("Session-based Trading (stickiness): IP Hash");
Console.WriteLine("CDN / Mixed Capacity: Weighted Round-Robin");
Console.WriteLine("Uneven transient workloads (dynamic connections): Least Connections\n");

Console.WriteLine("Done.");
