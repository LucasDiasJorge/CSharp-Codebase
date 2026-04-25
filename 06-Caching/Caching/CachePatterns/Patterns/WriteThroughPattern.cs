using Microsoft.Extensions.Caching.Memory;
using CachePatterns.Models;
using CachePatterns.Data;

namespace CachePatterns.Patterns;

/// <summary>
/// 2. Write-Through
/// Escreve simultaneamente no cache e no banco de dados
/// Garante que cache e DB estejam sempre sincronizados
/// </summary>
public class WriteThroughService
{
    private readonly IMemoryCache _cache;
    private readonly IRepository<Produto> _repository;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

    public WriteThroughService(IMemoryCache cache, IRepository<Produto> repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        var cacheKey = $"writethrough:produto:{id}";
        
        if (_cache.TryGetValue(cacheKey, out Produto? produto))
        {
            Console.WriteLine($"[WRITE-THROUGH CACHE HIT] Produto {id} encontrado no cache");
            return produto;
        }

        Console.WriteLine($"[WRITE-THROUGH CACHE MISS] Buscando produto {id} no banco");
        produto = await _repository.GetByIdAsync(id);
        
        if (produto != null)
        {
            _cache.Set(cacheKey, produto, _defaultExpiration);
            Console.WriteLine($"[WRITE-THROUGH CACHE SET] Produto {id} adicionado ao cache");
        }

        return produto;
    }

    public async Task SaveProdutoAsync(Produto produto)
    {
        var cacheKey = $"writethrough:produto:{produto.Id}";
        
        try
        {
            Console.WriteLine($"[WRITE-THROUGH] Iniciando escrita simultânea para produto {produto.Id}");
            
            // Cria as tarefas para execução paralela (opcional)
            var dbTask = _repository.SaveAsync(produto);
            var cacheTask = Task.Run(() => 
            {
                _cache.Set(cacheKey, produto, _defaultExpiration);
                Console.WriteLine($"[WRITE-THROUGH CACHE] Produto {produto.Id} salvo no cache");
            });

            // Aguarda ambas as operações completarem
            await Task.WhenAll(dbTask, cacheTask);
            
            Console.WriteLine($"[WRITE-THROUGH] Produto {produto.Id} salvo com sucesso no DB e Cache");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WRITE-THROUGH ERROR] Erro ao salvar produto {produto.Id}: {ex.Message}");
            
            // Remove do cache em caso de erro no banco
            _cache.Remove(cacheKey);
            throw;
        }
    }

    public async Task DeleteProdutoAsync(int id)
    {
        var cacheKey = $"writethrough:produto:{id}";
        
        try
        {
            Console.WriteLine($"[WRITE-THROUGH] Iniciando remoção simultânea para produto {id}");
            
            var dbTask = _repository.DeleteAsync(id);
            var cacheTask = Task.Run(() => 
            {
                _cache.Remove(cacheKey);
                Console.WriteLine($"[WRITE-THROUGH CACHE] Produto {id} removido do cache");
            });

            await Task.WhenAll(dbTask, cacheTask);
            
            Console.WriteLine($"[WRITE-THROUGH] Produto {id} removido com sucesso do DB e Cache");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WRITE-THROUGH ERROR] Erro ao remover produto {id}: {ex.Message}");
            throw;
        }
    }
}
