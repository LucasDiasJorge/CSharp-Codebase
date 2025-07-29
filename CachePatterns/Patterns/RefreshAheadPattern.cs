using Microsoft.Extensions.Caching.Memory;
using CachePatterns.Models;
using CachePatterns.Data;
using System.Collections.Concurrent;

namespace CachePatterns.Patterns;

/// <summary>
/// 5. Refresh-Ahead
/// Atualiza o cache proativamente antes da expiração
/// Evita cache miss em dados críticos
/// </summary>
public class RefreshAheadService : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly IRepository<Produto> _repository;
    private readonly Timer _refreshTimer;
    private readonly ConcurrentDictionary<string, DateTime> _cacheMetadata;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _refreshThreshold = TimeSpan.FromMinutes(2); // Refresh quando restam 2 min
    private readonly SemaphoreSlim _refreshSemaphore;
    private bool _disposed = false;

    public RefreshAheadService(IMemoryCache cache, IRepository<Produto> repository)
    {
        _cache = cache;
        _repository = repository;
        _cacheMetadata = new ConcurrentDictionary<string, DateTime>();
        _refreshSemaphore = new SemaphoreSlim(1, 1);
        
        // Timer para verificação periódica (a cada 30 segundos)
        _refreshTimer = new Timer(CheckAndRefreshCache, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        var cacheKey = $"refreshahead:produto:{id}";
        
        if (_cache.TryGetValue(cacheKey, out Produto? produto))
        {
            Console.WriteLine($"[REFRESH-AHEAD CACHE HIT] Produto {id} encontrado no cache");
            
            // Verifica se precisa de refresh proativo
            _ = Task.Run(() => CheckForProactiveRefresh(cacheKey, id));
            
            return produto;
        }

        Console.WriteLine($"[REFRESH-AHEAD CACHE MISS] Produto {id} não encontrado, carregando do banco");
        
        produto = await _repository.GetByIdAsync(id);
        
        if (produto != null)
        {
            SetInCache(cacheKey, produto);
        }

        return produto;
    }

    private void SetInCache(string cacheKey, Produto produto)
    {
        var expirationTime = DateTime.Now.Add(_cacheExpiration);
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration,
            Priority = CacheItemPriority.High,
            PostEvictionCallbacks = { new PostEvictionCallbackRegistration
            {
                EvictionCallback = (key, value, reason, state) =>
                {
                    _cacheMetadata.TryRemove(key.ToString()!, out _);
                    Console.WriteLine($"[REFRESH-AHEAD EVICT] {key} removido do cache. Motivo: {reason}");
                }
            }}
        };

        _cache.Set(cacheKey, produto, cacheOptions);
        _cacheMetadata.AddOrUpdate(cacheKey, expirationTime, (k, v) => expirationTime);
        
        Console.WriteLine($"[REFRESH-AHEAD CACHE SET] Produto {produto.Id} adicionado ao cache até {expirationTime:HH:mm:ss}");
    }

    private async Task CheckForProactiveRefresh(string cacheKey, int id)
    {
        if (!_cacheMetadata.TryGetValue(cacheKey, out var expirationTime))
            return;

        var timeUntilExpiration = expirationTime - DateTime.Now;
        
        if (timeUntilExpiration <= _refreshThreshold && timeUntilExpiration > TimeSpan.Zero)
        {
            Console.WriteLine($"[REFRESH-AHEAD PROACTIVE] Produto {id} será expirado em {timeUntilExpiration.TotalMinutes:F1} min, iniciando refresh");
            await RefreshCacheEntry(cacheKey, id);
        }
    }

    private async void CheckAndRefreshCache(object? state)
    {
        if (!await _refreshSemaphore.WaitAsync(100))
            return;

        try
        {
            var now = DateTime.Now;
            var keysToRefresh = _cacheMetadata
                .Where(kvp => kvp.Value - now <= _refreshThreshold && kvp.Value > now)
                .Select(kvp => kvp.Key)
                .ToList();

            if (keysToRefresh.Count > 0)
            {
                Console.WriteLine($"[REFRESH-AHEAD BATCH] Iniciando refresh de {keysToRefresh.Count} entradas");
                
                var refreshTasks = keysToRefresh.Select(async cacheKey =>
                {
                    if (cacheKey.Contains("produto:"))
                    {
                        var idStr = cacheKey.Split(':').Last();
                        if (int.TryParse(idStr, out var id))
                        {
                            await RefreshCacheEntry(cacheKey, id);
                        }
                    }
                });

                await Task.WhenAll(refreshTasks);
            }
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private async Task RefreshCacheEntry(string cacheKey, int id)
    {
        try
        {
            Console.WriteLine($"[REFRESH-AHEAD REFRESH] Atualizando produto {id} no cache");
            
            var produto = await _repository.GetByIdAsync(id);
            
            if (produto != null)
            {
                SetInCache(cacheKey, produto);
                Console.WriteLine($"[REFRESH-AHEAD SUCCESS] Produto {id} atualizado com sucesso");
            }
            else
            {
                Console.WriteLine($"[REFRESH-AHEAD MISS] Produto {id} não encontrado no banco durante refresh");
                // Remove do cache se não existe mais no banco
                _cache.Remove(cacheKey);
                _cacheMetadata.TryRemove(cacheKey, out _);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REFRESH-AHEAD ERROR] Erro ao atualizar produto {id}: {ex.Message}");
        }
    }

    // Método para forçar refresh manual de uma entrada específica
    public async Task ForceRefreshAsync(int id)
    {
        var cacheKey = $"refreshahead:produto:{id}";
        Console.WriteLine($"[REFRESH-AHEAD MANUAL] Forçando refresh do produto {id}");
        await RefreshCacheEntry(cacheKey, id);
    }

    // Método para pré-carregar dados críticos
    public async Task PreloadCriticalDataAsync(IEnumerable<int> criticalIds)
    {
        Console.WriteLine($"[REFRESH-AHEAD PRELOAD] Pré-carregando {criticalIds.Count()} produtos críticos");
        
        var preloadTasks = criticalIds.Select(async id =>
        {
            var produto = await _repository.GetByIdAsync(id);
            if (produto != null)
            {
                var cacheKey = $"refreshahead:produto:{id}";
                SetInCache(cacheKey, produto);
            }
        });

        await Task.WhenAll(preloadTasks);
        Console.WriteLine("[REFRESH-AHEAD PRELOAD] Pré-carregamento concluído");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _refreshTimer?.Dispose();
            _refreshSemaphore?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
