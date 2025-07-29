using Microsoft.Extensions.Caching.Memory;
using CachePatterns.Models;
using CachePatterns.Data;

namespace CachePatterns.Patterns;

/// <summary>
/// 6. Full Cache / Cache-First
/// Carrega todo o dataset ou grandes blocos na inicialização
/// Ideal para dados imutáveis ou que mudam pouco
/// </summary>
public class FullCacheService : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly IRepository<Produto> _repository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly Timer _refreshTimer;
    private readonly SemaphoreSlim _refreshSemaphore;
    private bool _disposed = false;
    private bool _isInitialized = false;

    public FullCacheService(
        IMemoryCache cache, 
        IRepository<Produto> repository,
        IRepository<Usuario> usuarioRepository)
    {
        _cache = cache;
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _refreshSemaphore = new SemaphoreSlim(1, 1);
        
        // Timer para refresh completo periódico (a cada 30 minutos)
        _refreshTimer = new Timer(RefreshFullCache, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        await _refreshSemaphore.WaitAsync();
        try
        {
            if (_isInitialized)
                return;

            Console.WriteLine("[FULL CACHE] Iniciando carregamento completo do cache...");
            
            await LoadAllDataAsync();
            
            _isInitialized = true;
            Console.WriteLine("[FULL CACHE] Inicialização completa!");
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private async Task LoadAllDataAsync()
    {
        // Carrega todos os produtos
        var produtos = await _repository.GetAllAsync();
        var produtosList = produtos.ToList();
        
        Console.WriteLine($"[FULL CACHE] Carregando {produtosList.Count} produtos...");
        
        foreach (var produto in produtosList)
        {
            var cacheKey = $"fullcache:produto:{produto.Id}";
            _cache.Set(cacheKey, produto, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove, // Dados críticos nunca são removidos
                SlidingExpiration = null // Sem expiração automática
            });
        }

        // Carrega todos os usuários
        var usuarios = await _usuarioRepository.GetAllAsync();
        var usuariosList = usuarios.ToList();
        
        Console.WriteLine($"[FULL CACHE] Carregando {usuariosList.Count} usuários...");
        
        foreach (var usuario in usuariosList)
        {
            var cacheKey = $"fullcache:usuario:{usuario.Id}";
            _cache.Set(cacheKey, usuario, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.High,
                SlidingExpiration = null
            });
        }

        // Carrega dados de configuração (simulação)
        await LoadConfigurationDataAsync();
        
        // Cria índices em memória para pesquisas rápidas
        await CreateInMemoryIndexesAsync(produtosList, usuariosList);
    }

    private async Task LoadConfigurationDataAsync()
    {
        // Simula carregamento de dados de configuração que raramente mudam
        var configuracoes = new[]
        {
            new Configuracao { Chave = "taxa_imposto", Valor = "0.18", UltimaAtualizacao = DateTime.Now },
            new Configuracao { Chave = "moeda_padrao", Valor = "BRL", UltimaAtualizacao = DateTime.Now },
            new Configuracao { Chave = "desconto_maximo", Valor = "0.30", UltimaAtualizacao = DateTime.Now }
        };

        Console.WriteLine($"[FULL CACHE] Carregando {configuracoes.Length} configurações...");
        
        foreach (var config in configuracoes)
        {
            var cacheKey = $"fullcache:config:{config.Chave}";
            _cache.Set(cacheKey, config, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            });
        }

        await Task.Delay(50); // Simula processamento
    }

    private async Task CreateInMemoryIndexesAsync(List<Produto> produtos, List<Usuario> usuarios)
    {
        Console.WriteLine("[FULL CACHE] Criando índices em memória...");
        
        // Índice de produtos por categoria
        var produtosPorCategoria = produtos.GroupBy(p => p.Categoria).ToDictionary(g => g.Key, g => g.ToList());
        _cache.Set("fullcache:index:produtos_por_categoria", produtosPorCategoria, new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.High
        });

        // Índice de produtos por faixa de preço
        var produtosPorFaixaPreco = new Dictionary<string, List<Produto>>
        {
            ["baixo"] = produtos.Where(p => p.Preco < 200).ToList(),
            ["medio"] = produtos.Where(p => p.Preco >= 200 && p.Preco < 1000).ToList(),
            ["alto"] = produtos.Where(p => p.Preco >= 1000).ToList()
        };
        _cache.Set("fullcache:index:produtos_por_preco", produtosPorFaixaPreco, new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.High
        });

        Console.WriteLine("[FULL CACHE] Índices criados com sucesso");
        await Task.Delay(50);
    }

    // Métodos de consulta que sempre usam cache
    public async Task<Produto?> GetProdutoAsync(int id)
    {
        await InitializeAsync();
        
        var cacheKey = $"fullcache:produto:{id}";
        
        if (_cache.TryGetValue(cacheKey, out Produto? produto))
        {
            Console.WriteLine($"[FULL CACHE HIT] Produto {id} encontrado no cache");
            return produto;
        }

        Console.WriteLine($"[FULL CACHE MISS] Produto {id} não encontrado no cache (pode ser novo)");
        return null;
    }

    public async Task<List<Produto>> GetProdutosPorCategoriaAsync(string categoria)
    {
        await InitializeAsync();
        
        if (_cache.TryGetValue("fullcache:index:produtos_por_categoria", out Dictionary<string, List<Produto>>? index))
        {
            if (index!.TryGetValue(categoria, out var produtos))
            {
                Console.WriteLine($"[FULL CACHE INDEX HIT] {produtos.Count} produtos encontrados na categoria {categoria}");
                return produtos;
            }
        }

        Console.WriteLine($"[FULL CACHE INDEX MISS] Categoria {categoria} não encontrada");
        return new List<Produto>();
    }

    public async Task<List<Produto>> GetProdutosPorFaixaPrecoAsync(string faixa)
    {
        await InitializeAsync();
        
        if (_cache.TryGetValue("fullcache:index:produtos_por_preco", out Dictionary<string, List<Produto>>? index))
        {
            if (index!.TryGetValue(faixa.ToLower(), out var produtos))
            {
                Console.WriteLine($"[FULL CACHE INDEX HIT] {produtos.Count} produtos encontrados na faixa {faixa}");
                return produtos;
            }
        }

        Console.WriteLine($"[FULL CACHE INDEX MISS] Faixa de preço {faixa} não encontrada");
        return new List<Produto>();
    }

    public async Task<Configuracao?> GetConfiguracaoAsync(string chave)
    {
        await InitializeAsync();
        
        var cacheKey = $"fullcache:config:{chave}";
        
        if (_cache.TryGetValue(cacheKey, out Configuracao? config))
        {
            Console.WriteLine($"[FULL CACHE CONFIG HIT] Configuração {chave} encontrada");
            return config;
        }

        Console.WriteLine($"[FULL CACHE CONFIG MISS] Configuração {chave} não encontrada");
        return null;
    }

    // Refresh completo do cache
    private async void RefreshFullCache(object? state)
    {
        Console.WriteLine("[FULL CACHE] Iniciando refresh completo periódico...");
        await RefreshFullCacheAsync();
    }

    public async Task RefreshFullCacheAsync()
    {
        if (!await _refreshSemaphore.WaitAsync(1000))
        {
            Console.WriteLine("[FULL CACHE] Refresh já em andamento, pulando...");
            return;
        }

        try
        {
            Console.WriteLine("[FULL CACHE] Executando refresh completo...");
            
            // Limpa cache atual
            ClearFullCache();
            
            // Recarrega todos os dados
            await LoadAllDataAsync();
            
            Console.WriteLine("[FULL CACHE] Refresh completo finalizado");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FULL CACHE ERROR] Erro durante refresh: {ex.Message}");
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private void ClearFullCache()
    {
        // Remove entradas do full cache
        var keysToRemove = new List<string>();
        
        // Aqui você poderia implementar uma lógica mais sofisticada
        // para identificar e remover apenas as chaves do full cache
        
        Console.WriteLine("[FULL CACHE] Cache limpo para refresh");
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
