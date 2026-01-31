# Load Balancing Algorithms

Implements and compares four classic load balancing strategies:

- Round-Robin
- Least Connections
- Weighted Round-Robin
- IP Hash

## Algorithms

| Algorithm | When to Use | Strengths | Limitations |
|-----------|-------------|-----------|-------------|
| Round-Robin | Homogeneous servers, stateless requests | Very simple, fair spread | Ignores current load, capacity differences |
| Least Connections | Long-lived uneven connections | Adapts to live load | Needs accurate active connection counts |
| Weighted Round-Robin | Heterogeneous server capacity | Honors relative capacity | Still ignores live connection spikes |
| IP Hash | Session stickiness required | Keeps user on same backend | Uneven distribution if IP clusters |

## Scenarios (from instructions)

1. E-Commerce flash sale: Round-Robin ✅
2. Trading platform with sessions: IP Hash ✅
3. Video streaming / CDN varying capacity: Weighted Round-Robin ✅
4. (Additional) Real-time chat with bursty users: Least Connections ✅

## Running

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\LoadBalancingAlgorithms"
dotnet run
```

## Output (excerpt)
```
=== Round-Robin ===
S1: requests=66 ...
S2: requests=67 ...
S3: requests=67 ...

=== Weighted Round-Robin ===
S1: requests≈33 (weight 1)
S2: requests≈66 (weight 2)
S3: requests≈100 (weight 3)
```

## Design

- `Models/Server.cs` – Tracks connections, weight, counters.
- `Models/Request.cs` – Simple request (IP + timestamp).
- `Algorithms/*` – One class per strategy implementing `ILoadBalancer`.
- `Program.cs` – Runs a deterministic simulation (seed = 42) for comparison.

## Extending

Add health checking, latency-aware routing, EWMA load scoring, or token-based rate shaping as next steps.

## License
Educational example; adjust freely for production use.
