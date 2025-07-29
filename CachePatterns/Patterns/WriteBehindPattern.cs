using Microsoft.Extensions.Caching.Memory;
using CachePatterns.Models;
using CachePatterns.Data;
using System.Collections.Concurrent;

namespace CachePatterns.Patterns;

/// <summary>
/// 3. Write-Behind (Write-Back)
/// Escreve primeiro no cache, depois persiste no banco de forma assíncrona
/// Melhora performance de escrita mas tem risco de perda de dados
/// </summary>
public class WriteBehindService : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly IRepository<Produto> _repository;
    private readonly ConcurrentQueue<(string action, Produto produto, DateTime timestamp)> _writeQueue;
    private readonly Timer _flushTimer;
    private readonly SemaphoreSlim _flushSemaphore;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(15);
    private bool _disposed = false;

    public WriteBehindService(IMemoryCache cache, IRepository<Produto> repository)
    {
        _cache = cache;
        _repository = repository;
        _writeQueue = new ConcurrentQueue<(string, Produto, DateTime)>();
        _flushSemaphore = new SemaphoreSlim(1, 1);
        
        // Timer para flush periódico (a cada 5 segundos)
        _flushTimer = new Timer(FlushToDatabase, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        var cacheKey = $"writebehind:produto:{id}";
        
        if (_cache.TryGetValue(cacheKey, out Produto? produto))
        {
            Console.WriteLine($"[WRITE-BEHIND CACHE HIT] Produto {id} encontrado no cache");
            return produto;
        }

        Console.WriteLine($"[WRITE-BEHIND CACHE MISS] Buscando produto {id} no banco");
        produto = await _repository.GetByIdAsync(id);
        
        if (produto != null)
        {
            _cache.Set(cacheKey, produto, _defaultExpiration);
            Console.WriteLine($"[WRITE-BEHIND CACHE SET] Produto {id} adicionado ao cache");
        }

        return produto;
    }

    public Task SaveProdutoAsync(Produto produto)
    {
        var cacheKey = $"writebehind:produto:{produto.Id}";
        
        // 1. Escreve imediatamente no cache
        _cache.Set(cacheKey, produto, _defaultExpiration);
        Console.WriteLine($"[WRITE-BEHIND CACHE] Produto {produto.Id} salvo no cache imediatamente");
        
        // 2. Adiciona na fila para persistência assíncrona
        _writeQueue.Enqueue(("save", produto, DateTime.Now));
        Console.WriteLine($"[WRITE-BEHIND QUEUE] Produto {produto.Id} adicionado à fila de persistência");
        
        return Task.CompletedTask;
    }

    public Task DeleteProdutoAsync(int id)
    {
        var cacheKey = $"writebehind:produto:{id}";
        
        // 1. Remove imediatamente do cache
        _cache.Remove(cacheKey);
        Console.WriteLine($"[WRITE-BEHIND CACHE] Produto {id} removido do cache imediatamente");
        
        // 2. Adiciona na fila para remoção assíncrona no banco
        var produto = new Produto { Id = id }; // Só precisamos do ID para delete
        _writeQueue.Enqueue(("delete", produto, DateTime.Now));
        Console.WriteLine($"[WRITE-BEHIND QUEUE] Produto {id} adicionado à fila de remoção");
        
        return Task.CompletedTask;
    }

    public async Task ForceFlushAsync()
    {
        Console.WriteLine("[WRITE-BEHIND] Forçando flush manual da fila");
        await FlushToDatabaseAsync();
    }

    private async void FlushToDatabase(object? state)
    {
        await FlushToDatabaseAsync();
    }

    private async Task FlushToDatabaseAsync()
    {
        if (!await _flushSemaphore.WaitAsync(100)) // Timeout de 100ms
            return;

        try
        {
            var itemsToProcess = new List<(string action, Produto produto, DateTime timestamp)>();
            
            // Drena a fila
            while (_writeQueue.TryDequeue(out var item))
            {
                itemsToProcess.Add(item);
            }

            if (itemsToProcess.Count == 0)
                return;

            Console.WriteLine($"[WRITE-BEHIND FLUSH] Processando {itemsToProcess.Count} itens da fila");

            // Agrupa por ação e produto para otimizar
            var grouped = itemsToProcess
                .GroupBy(x => new { x.action, x.produto.Id })
                .Select(g => g.OrderByDescending(x => x.timestamp).First()) // Pega a operação mais recente
                .ToList();

            foreach (var item in grouped)
            {
                try
                {
                    switch (item.action)
                    {
                        case "save":
                            await _repository.SaveAsync(item.produto);
                            Console.WriteLine($"[WRITE-BEHIND DB] Produto {item.produto.Id} persistido no banco");
                            break;
                        
                        case "delete":
                            await _repository.DeleteAsync(item.produto.Id);
                            Console.WriteLine($"[WRITE-BEHIND DB] Produto {item.produto.Id} removido do banco");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WRITE-BEHIND ERROR] Erro ao processar {item.action} do produto {item.produto.Id}: {ex.Message}");
                    
                    // Recoloca na fila para retry (opcional)
                    _writeQueue.Enqueue(item);
                }
            }
        }
        finally
        {
            _flushSemaphore.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _flushTimer?.Dispose();
            
            // Flush final antes de descartar
            FlushToDatabaseAsync().Wait(TimeSpan.FromSeconds(10));
            
            _flushSemaphore?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
