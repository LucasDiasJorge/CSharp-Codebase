using DistributedCacheInvalidationDemo.Data;
using StackExchange.Redis;

namespace DistributedCacheInvalidationDemo.Cache;

/// <summary>
/// Os nós simulados da aplicação. Cada um tem cache local próprio e assinatura própria
/// no Redis — a mesma situação de três réplicas em produção, sem exigir três processos
/// para rodar o exemplo.
/// </summary>
public sealed class NodeCluster : IAsyncDisposable
{
    private readonly Dictionary<string, CacheNode> _nodes = new Dictionary<string, CacheNode>(StringComparer.OrdinalIgnoreCase);

    public NodeCluster(IConnectionMultiplexer redis, ProductRepository repository, ILoggerFactory loggerFactory)
    {
        foreach (string nodeId in new[] { "A", "B", "C" })
        {
            _nodes[nodeId] = new CacheNode(nodeId, redis, repository, loggerFactory.CreateLogger($"No{nodeId}"));
        }
    }

    public IReadOnlyCollection<CacheNode> Nodes => _nodes.Values;

    public CacheNode? Find(string nodeId) => _nodes.TryGetValue(nodeId, out CacheNode? node) ? node : null;

    public async Task SubscribeAllAsync()
    {
        foreach (CacheNode node in _nodes.Values)
        {
            await node.SubscribeAsync().ConfigureAwait(false);
        }
    }

    public void ClearAllLocal()
    {
        foreach (CacheNode node in _nodes.Values)
        {
            node.ClearLocal();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (CacheNode node in _nodes.Values)
        {
            await node.DisposeAsync().ConfigureAwait(false);
        }
    }
}
