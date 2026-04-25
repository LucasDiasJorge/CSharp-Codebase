namespace LoadBalancingAlgorithms.Models;

public class Server
{
    public required string Id { get; init; }
    public int ActiveConnections { get; set; }
    public int Weight { get; init; } = 1;
    public int HandledRequests { get; set; }

    public override string ToString() => $"{Id} (conn={ActiveConnections}, weight={Weight}, handled={HandledRequests})";
}
