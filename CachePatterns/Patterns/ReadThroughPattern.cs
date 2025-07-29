using Microsoft.Extensions.Caching.Memory;
using CachePatterns.Models;
using CachePatterns.Data;

namespace CachePatterns.Patterns;

/// <summary>
/// 4. Read-Through
/// A aplicação nunca acessa o banco diretamente
/// O cache automaticamente busca no banco em caso de miss
/// </summary>
public class ReadThroughService
{
    private readonly IMemoryCache _cache;
    private readonly IRepository<Produto> _repository;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(8);

    public ReadThroughService(IMemoryCache cache, IRepository<Produto> repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        var cacheKey = $"readthrough:produto:{id}";
        
        // Usa GetOrCreateAsync que implementa o padrão Read-Through
        var produto = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            Console.WriteLine($"[READ-THROUGH CACHE MISS] Produto {id} não encontrado, buscando no banco");
            
            entry.AbsoluteExpirationRelativeToNow = _defaultExpiration;
            entry.SlidingExpiration = TimeSpan.FromMinutes(3);
            entry.Priority = CacheItemPriority.Normal;
            
            // Esta função só é chamada em caso de cache miss
            var produtoFromDb = await _repository.GetByIdAsync(id);
            
            if (produtoFromDb != null)
            {
                Console.WriteLine($"[READ-THROUGH CACHE SET] Produto {id} carregado do banco e adicionado ao cache");
            }
            else
            {
                Console.WriteLine($"[READ-THROUGH] Produto {id} não encontrado no banco");
                // Define uma expiração menor para valores nulos para retry mais rápido
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            }
            
            return produtoFromDb;
        });

        if (produto != null)
        {
            Console.WriteLine($"[READ-THROUGH CACHE HIT] Produto {id} retornado do cache");
        }

        return produto;
    }

    // Versão alternativa com controle manual mais explícito
    public async Task<Produto?> GetProdutoManualAsync(int id)
    {
        var cacheKey = $"readthrough:produto:{id}";
        
        if (_cache.TryGetValue(cacheKey, out Produto? produto))
        {
            Console.WriteLine($"[READ-THROUGH MANUAL CACHE HIT] Produto {id} encontrado no cache");
            return produto;
        }

        Console.WriteLine($"[READ-THROUGH MANUAL CACHE MISS] Produto {id} não encontrado, executando read-through");
        
        // Read-through: cache busca no banco
        produto = await LoadFromDataSource(id);
        
        if (produto != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _defaultExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(3),
                Priority = CacheItemPriority.Normal
            };
            
            _cache.Set(cacheKey, produto, cacheOptions);
            Console.WriteLine($"[READ-THROUGH MANUAL CACHE SET] Produto {id} adicionado ao cache");
        }

        return produto;
    }

    // Método que encapsula a lógica de busca no banco (simula uma camada de cache inteligente)
    private async Task<Produto?> LoadFromDataSource(int id)
    {
        Console.WriteLine($"[READ-THROUGH DATA SOURCE] Carregando produto {id} do banco de dados");
        return await _repository.GetByIdAsync(id);
    }

    // Para invalidação manual quando necessário
    public void InvalidateCache(int id)
    {
        var cacheKey = $"readthrough:produto:{id}";
        _cache.Remove(cacheKey);
        Console.WriteLine($"[READ-THROUGH INVALIDATE] Cache do produto {id} invalidado");
    }

    // Busca com fallback para múltiplas fontes
    public async Task<Produto?> GetProdutoWithFallbackAsync(int id)
    {
        var cacheKey = $"readthrough:produto:{id}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _defaultExpiration;
            
            Console.WriteLine($"[READ-THROUGH FALLBACK] Tentando carregar produto {id}");
            
            // Tenta fonte primária
            var produto = await _repository.GetByIdAsync(id);
            
            if (produto == null)
            {
                Console.WriteLine($"[READ-THROUGH FALLBACK] Produto {id} não encontrado na fonte primária, tentando fallback");
                
                // Aqui poderia tentar uma fonte secundária, API externa, etc.
                // produto = await _secondaryRepository.GetByIdAsync(id);
                
                // Define expiração menor para retry mais rápido em caso de falha
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            }
            
            return produto;
        });
    }
}
