using Microsoft.Extensions.Caching.Memory;
using CachePatterns.Models;
using CachePatterns.Data;

namespace CachePatterns.Patterns;

/// <summary>
/// 1. Cache-Aside (Lazy-Loading)
/// A aplicação gerencia o cache manualmente
/// Lê do cache primeiro, se não encontra vai no DB e atualiza o cache
/// </summary>
public class CacheAsideService
{
    private readonly IMemoryCache _cache;
    private readonly IRepository<Produto> _repository;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

    public CacheAsideService(IMemoryCache cache, IRepository<Produto> repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        var cacheKey = $"produto:{id}";
        
        // 1. Primeiro tenta buscar no cache
        if (_cache.TryGetValue(cacheKey, out Produto? produto))
        {
            Console.WriteLine($"[CACHE HIT] Produto {id} encontrado no cache");
            return produto;
        }

        Console.WriteLine($"[CACHE MISS] Produto {id} não encontrado no cache");
        
        // 2. Se não encontrou, busca no banco
        produto = await _repository.GetByIdAsync(id);
        
        // 3. Se encontrou no banco, adiciona ao cache
        if (produto != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _defaultExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(2), // Renova se acessado
                Priority = CacheItemPriority.Normal
            };
            
            _cache.Set(cacheKey, produto, cacheOptions);
            Console.WriteLine($"[CACHE SET] Produto {id} adicionado ao cache");
        }

        return produto;
    }

    public async Task SaveProdutoAsync(Produto produto)
    {
        // Salva no banco
        await _repository.SaveAsync(produto);
        
        // Remove do cache para forçar reload na próxima consulta
        var cacheKey = $"produto:{produto.Id}";
        _cache.Remove(cacheKey);
        Console.WriteLine($"[CACHE INVALIDATE] Produto {produto.Id} removido do cache");
    }

    public void InvalidateCache(int id)
    {
        var cacheKey = $"produto:{id}";
        _cache.Remove(cacheKey);
        Console.WriteLine($"[CACHE INVALIDATE] Cache do produto {id} invalidado");
    }
}
