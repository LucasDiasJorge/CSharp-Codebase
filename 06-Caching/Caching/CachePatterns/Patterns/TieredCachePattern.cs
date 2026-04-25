using Microsoft.Extensions.Caching.Memory;
using CachePatterns.Models;
using CachePatterns.Data;

namespace CachePatterns.Patterns;

/// <summary>
/// 8. Tiered Caching (Multi-Level Cache)
/// Usa múltiplos níveis de cache para otimizar performance
/// L1: Cache em memória local (mais rápido)
/// L2: Cache distribuído (Redis simulado)
/// L3: Banco de dados
/// </summary>
public class TieredCacheService : IDisposable
{
    private readonly IMemoryCache _l1Cache; // Cache local em memória
    private readonly IMemoryCache _l2Cache; // Simula cache distribuído (Redis)
    private readonly IRepository<Produto> _repository; // L3 - Banco de dados
    
    private readonly TimeSpan _l1Expiration = TimeSpan.FromMinutes(2);  // Cache local mais volátil
    private readonly TimeSpan _l2Expiration = TimeSpan.FromMinutes(10); // Cache distribuído mais duradouro
    
    private readonly string _instanceId;
    private bool _disposed = false;

    // Métricas para análise de performance
    private long _l1Hits = 0;
    private long _l2Hits = 0;
    private long _l3Hits = 0;
    private long _totalRequests = 0;

    public TieredCacheService(
        IMemoryCache l1Cache, 
        IMemoryCache l2Cache, 
        IRepository<Produto> repository)
    {
        _l1Cache = l1Cache;
        _l2Cache = l2Cache;
        _repository = repository;
        _instanceId = Environment.MachineName + "_" + Guid.NewGuid().ToString("N")[..6];
        
        Console.WriteLine($"[TIERED CACHE] Instância {_instanceId} inicializada com cache multi-nível");
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        Interlocked.Increment(ref _totalRequests);
        var cacheKey = $"produto:{id}";
        
        // NÍVEL 1: Cache local em memória (L1) - mais rápido
        if (_l1Cache.TryGetValue(cacheKey, out Produto? produto))
        {
            Interlocked.Increment(ref _l1Hits);
            Console.WriteLine($"[L1 CACHE HIT] [{_instanceId}] Produto {id} encontrado no cache L1 (local)");
            return produto;
        }

        Console.WriteLine($"[L1 CACHE MISS] [{_instanceId}] Produto {id} não encontrado no L1");

        // NÍVEL 2: Cache distribuído (L2) - latência média
        if (_l2Cache.TryGetValue(cacheKey, out produto))
        {
            Interlocked.Increment(ref _l2Hits);
            Console.WriteLine($"[L2 CACHE HIT] [{_instanceId}] Produto {id} encontrado no cache L2 (distribuído)");
            
            // Promove para L1 para próximos acessos
            PromoteToL1(cacheKey, produto);
            return produto;
        }

        Console.WriteLine($"[L2 CACHE MISS] [{_instanceId}] Produto {id} não encontrado no L2");

        // NÍVEL 3: Banco de dados (L3) - mais lento
        produto = await GetFromDatabaseAsync(id);
        
        if (produto != null)
        {
            Interlocked.Increment(ref _l3Hits);
            Console.WriteLine($"[L3 DATABASE HIT] [{_instanceId}] Produto {id} carregado do banco de dados");
            
            // Armazena em todos os níveis de cache
            await StoreInAllLevelsAsync(cacheKey, produto);
        }
        else
        {
            Console.WriteLine($"[L3 DATABASE MISS] [{_instanceId}] Produto {id} não encontrado no banco");
        }

        return produto;
    }

    private async Task<Produto?> GetFromDatabaseAsync(int id)
    {
        // Simula latência de rede/disco
        await Task.Delay(100);
        return await _repository.GetByIdAsync(id);
    }

    private void PromoteToL1(string cacheKey, Produto produto)
    {
        var l1Options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _l1Expiration,
            Priority = CacheItemPriority.High,
            Size = 1 // Para controle de memória
        };

        _l1Cache.Set(cacheKey, produto, l1Options);
        Console.WriteLine($"[L1 PROMOTION] [{_instanceId}] Produto {produto.Id} promovido para cache L1");
    }

    private async Task StoreInAllLevelsAsync(string cacheKey, Produto produto)
    {
        // L2 Cache (distribuído) - expira depois
        var l2Options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _l2Expiration,
            Priority = CacheItemPriority.Normal,
            Size = 1
        };

        _l2Cache.Set(cacheKey, produto, l2Options);
        Console.WriteLine($"[L2 CACHE SET] Produto {produto.Id} armazenado no cache L2");

        // L1 Cache (local) - expira primeiro
        PromoteToL1(cacheKey, produto);
        
        await Task.CompletedTask;
    }

    public async Task SaveProdutoAsync(Produto produto)
    {
        var cacheKey = $"produto:{produto.Id}";
        
        // Salva no banco de dados
        await _repository.SaveAsync(produto);
        Console.WriteLine($"[TIERED CACHE SAVE] [{_instanceId}] Produto {produto.Id} salvo no banco");
        
        // Estratégia: Invalidar em todos os níveis e recarregar
        InvalidateAllLevels(cacheKey);
        
        // Opcionalmente, pode recarregar nos caches para próximo acesso
        await StoreInAllLevelsAsync(cacheKey, produto);
    }

    public async Task DeleteProdutoAsync(int id)
    {
        var cacheKey = $"produto:{id}";
        
        // Remove do banco
        await _repository.DeleteAsync(id);
        Console.WriteLine($"[TIERED CACHE DELETE] [{_instanceId}] Produto {id} removido do banco");
        
        // Remove de todos os níveis de cache
        InvalidateAllLevels(cacheKey);
    }

    private void InvalidateAllLevels(string cacheKey)
    {
        _l1Cache.Remove(cacheKey);
        _l2Cache.Remove(cacheKey);
        Console.WriteLine($"[TIERED CACHE INVALIDATE] Cache invalidado em todos os níveis para {cacheKey}");
    }

    // Método para pré-aquecimento de cache (warm-up)
    public async Task WarmUpCacheAsync(IEnumerable<int> criticalIds)
    {
        Console.WriteLine($"[TIERED CACHE WARMUP] [{_instanceId}] Iniciando aquecimento de cache...");
        
        var warmupTasks = criticalIds.Select(async id =>
        {
            var produto = await GetFromDatabaseAsync(id);
            if (produto != null)
            {
                var cacheKey = $"produto:{id}";
                await StoreInAllLevelsAsync(cacheKey, produto);
            }
        });

        await Task.WhenAll(warmupTasks);
        Console.WriteLine($"[TIERED CACHE WARMUP] [{_instanceId}] Aquecimento concluído");
    }

    // Busca em lote otimizada
    public async Task<List<Produto>> GetProdutosBatchAsync(IEnumerable<int> ids)
    {
        var idsList = ids.ToList();
        var result = new List<Produto>();
        var missedIds = new List<int>();

        Console.WriteLine($"[TIERED CACHE BATCH] [{_instanceId}] Buscando {idsList.Count} produtos em lote");

        // Primeira passada: busca em L1 e L2
        foreach (var id in idsList)
        {
            var cacheKey = $"produto:{id}";
            
            if (_l1Cache.TryGetValue(cacheKey, out Produto? produto))
            {
                result.Add(produto!);
                Interlocked.Increment(ref _l1Hits);
            }
            else if (_l2Cache.TryGetValue(cacheKey, out produto))
            {
                result.Add(produto!);
                PromoteToL1(cacheKey, produto);
                Interlocked.Increment(ref _l2Hits);
            }
            else
            {
                missedIds.Add(id);
            }
        }

        // Segunda passada: busca os que não foram encontrados no banco (L3)
        if (missedIds.Count > 0)
        {
            Console.WriteLine($"[TIERED CACHE BATCH] [{_instanceId}] {missedIds.Count} produtos não encontrados em cache, buscando no banco");
            
            var dbTasks = missedIds.Select(async id =>
            {
                var produto = await GetFromDatabaseAsync(id);
                if (produto != null)
                {
                    var cacheKey = $"produto:{id}";
                    await StoreInAllLevelsAsync(cacheKey, produto);
                    Interlocked.Increment(ref _l3Hits);
                    return produto;
                }
                return null;
            });

            var dbResults = await Task.WhenAll(dbTasks);
            result.AddRange(dbResults.Where(p => p != null)!);
        }

        Interlocked.Add(ref _totalRequests, idsList.Count);
        return result;
    }

    // Força refresh de um item específico em todos os níveis
    public async Task RefreshItemAsync(int id)
    {
        var cacheKey = $"produto:{id}";
        
        Console.WriteLine($"[TIERED CACHE REFRESH] [{_instanceId}] Forçando refresh do produto {id}");
        
        // Remove de todos os caches
        InvalidateAllLevels(cacheKey);
        
        // Recarrega do banco
        var produto = await GetFromDatabaseAsync(id);
        if (produto != null)
        {
            await StoreInAllLevelsAsync(cacheKey, produto);
        }
    }

    // Limpa caches específicos
    public void ClearL1Cache()
    {
        // Não há uma forma direta de limpar IMemoryCache, 
        // mas você pode manter uma lista de chaves ou usar uma implementação customizada
        Console.WriteLine($"[TIERED CACHE] [{_instanceId}] Cache L1 seria limpo (implementação específica necessária)");
    }

    // Métricas de performance
    public TieredCacheMetrics GetMetrics()
    {
        var total = _totalRequests;
        return new TieredCacheMetrics
        {
            InstanceId = _instanceId,
            TotalRequests = total,
            L1Hits = _l1Hits,
            L2Hits = _l2Hits,
            L3Hits = _l3Hits,
            L1HitRate = total > 0 ? (double)_l1Hits / total * 100 : 0,
            L2HitRate = total > 0 ? (double)_l2Hits / total * 100 : 0,
            L3HitRate = total > 0 ? (double)_l3Hits / total * 100 : 0,
            CacheHitRate = total > 0 ? (double)(_l1Hits + _l2Hits) / total * 100 : 0
        };
    }

    public void ResetMetrics()
    {
        _l1Hits = 0;
        _l2Hits = 0;
        _l3Hits = 0;
        _totalRequests = 0;
        Console.WriteLine($"[TIERED CACHE] [{_instanceId}] Métricas resetadas");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            var metrics = GetMetrics();
            Console.WriteLine($"[TIERED CACHE] [{_instanceId}] Estatísticas finais:");
            Console.WriteLine($"  Total de requisições: {metrics.TotalRequests}");
            Console.WriteLine($"  Taxa de hit L1: {metrics.L1HitRate:F2}%");
            Console.WriteLine($"  Taxa de hit L2: {metrics.L2HitRate:F2}%");
            Console.WriteLine($"  Taxa de hit geral: {metrics.CacheHitRate:F2}%");
            
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

public class TieredCacheMetrics
{
    public string InstanceId { get; set; } = string.Empty;
    public long TotalRequests { get; set; }
    public long L1Hits { get; set; }
    public long L2Hits { get; set; }
    public long L3Hits { get; set; }
    public double L1HitRate { get; set; }
    public double L2HitRate { get; set; }
    public double L3HitRate { get; set; }
    public double CacheHitRate { get; set; }
}
