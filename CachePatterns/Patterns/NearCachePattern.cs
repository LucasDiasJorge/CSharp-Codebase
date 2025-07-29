using Microsoft.Extensions.Caching.Memory;
using CachePatterns.Models;
using CachePatterns.Data;
using System.Collections.Concurrent;

namespace CachePatterns.Patterns;

/// <summary>
/// 7. Near Cache
/// Cada instância da aplicação mantém uma cópia local do cache
/// Usado em sistemas distribuídos para reduzir latência
/// </summary>
public class NearCacheService : IDisposable
{
    private readonly IMemoryCache _localCache; // Cache local (L1)
    private readonly IMemoryCache _remoteCache; // Simula cache remoto (Redis)
    private readonly IRepository<Produto> _repository;
    private readonly Timer _syncTimer;
    private readonly ConcurrentDictionary<string, DateTime> _localCacheTimestamps;
    private readonly TimeSpan _localCacheExpiration = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _remoteCacheExpiration = TimeSpan.FromMinutes(15);
    private readonly string _instanceId;
    private bool _disposed = false;

    public NearCacheService(
        IMemoryCache localCache, 
        IMemoryCache remoteCache, 
        IRepository<Produto> repository)
    {
        _localCache = localCache;
        _remoteCache = remoteCache;
        _repository = repository;
        _localCacheTimestamps = new ConcurrentDictionary<string, DateTime>();
        _instanceId = Environment.MachineName + "_" + Guid.NewGuid().ToString("N")[..8];
        
        // Timer para sincronização periódica (a cada 2 minutos)
        _syncTimer = new Timer(SyncWithRemoteCache, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        
        Console.WriteLine($"[NEAR CACHE] Instância {_instanceId} inicializada");
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        var cacheKey = $"produto:{id}";
        
        // 1. Primeiro verifica cache local (L1) - mais rápido
        if (_localCache.TryGetValue(cacheKey, out Produto? produto))
        {
            Console.WriteLine($"[NEAR CACHE L1 HIT] [{_instanceId}] Produto {id} encontrado no cache local");
            return produto;
        }

        // 2. Se não encontrou localmente, verifica cache remoto (L2)
        if (_remoteCache.TryGetValue(cacheKey, out produto))
        {
            Console.WriteLine($"[NEAR CACHE L2 HIT] [{_instanceId}] Produto {id} encontrado no cache remoto");
            
            // Copia para cache local para próximos acessos
            SetInLocalCache(cacheKey, produto);
            return produto;
        }

        Console.WriteLine($"[NEAR CACHE MISS] [{_instanceId}] Produto {id} não encontrado em nenhum cache");
        
        // 3. Se não encontrou em nenhum cache, busca no banco
        produto = await _repository.GetByIdAsync(id);
        
        if (produto != null)
        {
            // Salva em ambos os caches
            await SetInBothCachesAsync(cacheKey, produto);
        }

        return produto;
    }

    private void SetInLocalCache(string cacheKey, Produto produto)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _localCacheExpiration,
            Priority = CacheItemPriority.Normal,
            PostEvictionCallbacks = { new PostEvictionCallbackRegistration
            {
                EvictionCallback = (key, value, reason, state) =>
                {
                    _localCacheTimestamps.TryRemove(key.ToString()!, out _);
                    Console.WriteLine($"[NEAR CACHE L1 EVICT] [{_instanceId}] {key} removido do cache local");
                }
            }}
        };

        _localCache.Set(cacheKey, produto, cacheOptions);
        _localCacheTimestamps.AddOrUpdate(cacheKey, DateTime.Now, (k, v) => DateTime.Now);
        
        Console.WriteLine($"[NEAR CACHE L1 SET] [{_instanceId}] Produto {produto.Id} adicionado ao cache local");
    }

    private async Task SetInBothCachesAsync(string cacheKey, Produto produto)
    {
        // Cache remoto (simula Redis)
        var remoteCacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _remoteCacheExpiration,
            Priority = CacheItemPriority.High
        };
        
        _remoteCache.Set(cacheKey, produto, remoteCacheOptions);
        Console.WriteLine($"[NEAR CACHE L2 SET] Produto {produto.Id} adicionado ao cache remoto");
        
        // Cache local
        SetInLocalCache(cacheKey, produto);
        
        await Task.CompletedTask;
    }

    public async Task SaveProdutoAsync(Produto produto)
    {
        var cacheKey = $"produto:{produto.Id}";
        
        // Salva no banco
        await _repository.SaveAsync(produto);
        
        // Invalida em ambos os caches para manter consistência
        _localCache.Remove(cacheKey);
        _remoteCache.Remove(cacheKey);
        _localCacheTimestamps.TryRemove(cacheKey, out _);
        
        Console.WriteLine($"[NEAR CACHE INVALIDATE] [{_instanceId}] Produto {produto.Id} invalidado em ambos os caches");
        
        // Opcionalmente, pode recarregar nos caches
        await SetInBothCachesAsync(cacheKey, produto);
    }

    // Sincronização periódica com cache remoto
    private async void SyncWithRemoteCache(object? state)
    {
        await SyncWithRemoteCacheAsync();
    }

    public async Task SyncWithRemoteCacheAsync()
    {
        try
        {
            Console.WriteLine($"[NEAR CACHE SYNC] [{_instanceId}] Iniciando sincronização com cache remoto");
            
            var localKeys = _localCacheTimestamps.Keys.ToList();
            var syncCount = 0;

            foreach (var cacheKey in localKeys)
            {
                // Verifica se existe versão mais recente no cache remoto
                if (_remoteCache.TryGetValue(cacheKey, out Produto? remoteProduto))
                {
                    if (_localCache.TryGetValue(cacheKey, out Produto? localProduto))
                    {
                        // Compara timestamps para ver se precisa atualizar
                        if (remoteProduto!.UltimaAtualizacao > localProduto!.UltimaAtualizacao)
                        {
                            SetInLocalCache(cacheKey, remoteProduto);
                            syncCount++;
                            Console.WriteLine($"[NEAR CACHE SYNC] [{_instanceId}] {cacheKey} atualizado do cache remoto");
                        }
                    }
                }
                else
                {
                    // Se não existe no remoto mas existe localmente, pode ter sido invalidado
                    // Opcionalmente remove do local também
                    var keyParts = cacheKey.Split(':');
                    if (keyParts.Length == 2 && int.TryParse(keyParts[1], out var id))
                    {
                        var produtoFromDb = await _repository.GetByIdAsync(id);
                        if (produtoFromDb == null)
                        {
                            _localCache.Remove(cacheKey);
                            _localCacheTimestamps.TryRemove(cacheKey, out _);
                            Console.WriteLine($"[NEAR CACHE SYNC] [{_instanceId}] {cacheKey} removido do cache local (não existe mais)");
                        }
                    }
                }
            }

            if (syncCount > 0)
            {
                Console.WriteLine($"[NEAR CACHE SYNC] [{_instanceId}] Sincronização concluída - {syncCount} itens atualizados");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NEAR CACHE SYNC ERROR] [{_instanceId}] Erro durante sincronização: {ex.Message}");
        }
    }

    // Método para forçar reload de um item específico
    public async Task RefreshItemAsync(int id)
    {
        var cacheKey = $"produto:{id}";
        
        Console.WriteLine($"[NEAR CACHE REFRESH] [{_instanceId}] Forçando refresh do produto {id}");
        
        // Remove dos caches
        _localCache.Remove(cacheKey);
        _remoteCache.Remove(cacheKey);
        _localCacheTimestamps.TryRemove(cacheKey, out _);
        
        // Recarrega do banco
        var produto = await _repository.GetByIdAsync(id);
        if (produto != null)
        {
            await SetInBothCachesAsync(cacheKey, produto);
        }
    }

    // Estatísticas do cache local
    public NearCacheStats GetStats()
    {
        return new NearCacheStats
        {
            InstanceId = _instanceId,
            LocalCacheSize = _localCacheTimestamps.Count,
            OldestEntry = _localCacheTimestamps.Values.Any() ? _localCacheTimestamps.Values.Min() : (DateTime?)null,
            NewestEntry = _localCacheTimestamps.Values.Any() ? _localCacheTimestamps.Values.Max() : (DateTime?)null
        };
    }

    // Limpa cache local (mantém remoto)
    public void ClearLocalCache()
    {
        Console.WriteLine($"[NEAR CACHE CLEAR] [{_instanceId}] Limpando cache local");
        
        var keys = _localCacheTimestamps.Keys.ToList();
        foreach (var key in keys)
        {
            _localCache.Remove(key);
        }
        _localCacheTimestamps.Clear();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _syncTimer?.Dispose();
            Console.WriteLine($"[NEAR CACHE] [{_instanceId}] Instância descartada");
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

public class NearCacheStats
{
    public string InstanceId { get; set; } = string.Empty;
    public int LocalCacheSize { get; set; }
    public DateTime? OldestEntry { get; set; }
    public DateTime? NewestEntry { get; set; }
}
