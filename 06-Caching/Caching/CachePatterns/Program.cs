using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CachePatterns.Data;
using CachePatterns.Models;
using CachePatterns.Patterns;

namespace CachePatterns;

class Program
{
    static async Task Main(string[] args)
    {
        // Configuração de dependências
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IRepository<Produto>, ProdutoRepository>();
        services.AddSingleton<IRepository<Usuario>, UsuarioRepository>();
        
        var serviceProvider = services.BuildServiceProvider();
        var cache = serviceProvider.GetRequiredService<IMemoryCache>();
        var produtoRepo = serviceProvider.GetRequiredService<IRepository<Produto>>();
        var usuarioRepo = serviceProvider.GetRequiredService<IRepository<Usuario>>();

        Console.WriteLine("=== DEMONSTRAÇÃO DOS PADRÕES DE CACHE ===\n");

        // Demonstração de cada padrão
        await DemonstrateCacheAside(cache, produtoRepo);
        await DemonstrateWriteThrough(cache, produtoRepo);
        await DemonstrateWriteBehind(cache, produtoRepo);
        await DemonstrateReadThrough(cache, produtoRepo);
        await DemonstrateRefreshAhead(cache, produtoRepo);
        await DemonstrateFullCache(cache, produtoRepo, usuarioRepo);
        await DemonstrateNearCache(cache, produtoRepo);
        await DemonstrateTieredCache(cache, produtoRepo);

        Console.WriteLine("\n=== DEMONSTRAÇÃO CONCLUÍDA ===");
    }

    static async Task DemonstrateCacheAside(IMemoryCache cache, IRepository<Produto> repository)
    {
        Console.WriteLine("🔍 === CACHE-ASIDE PATTERN ===");
        var service = new CacheAsideService(cache, repository);

        // Primeira busca - cache miss
        await service.GetProdutoAsync(1);
        
        // Segunda busca - cache hit
        await service.GetProdutoAsync(1);
        
        // Simulação de atualização
        var produto = new Produto { Id = 1, Nome = "Notebook Dell Atualizado", Preco = 2600.00m, Categoria = "Eletrônicos" };
        await service.SaveProdutoAsync(produto);
        
        // Busca após atualização - cache miss (foi invalidado)
        await service.GetProdutoAsync(1);
        
        Console.WriteLine("✅ Cache-Aside demonstrado\n");
    }

    static async Task DemonstrateWriteThrough(IMemoryCache cache, IRepository<Produto> repository)
    {
        Console.WriteLine("🔄 === WRITE-THROUGH PATTERN ===");
        var service = new WriteThroughService(cache, repository);

        // Busca inicial
        await service.GetProdutoAsync(2);
        
        // Atualização simultânea em cache e DB
        var produto = new Produto { Id = 2, Nome = "Mouse Logitech Atualizado", Preco = 180.00m, Categoria = "Periféricos" };
        await service.SaveProdutoAsync(produto);
        
        // Verifica se a atualização está no cache
        await service.GetProdutoAsync(2);
        
        Console.WriteLine("✅ Write-Through demonstrado\n");
    }

    static async Task DemonstrateWriteBehind(IMemoryCache cache, IRepository<Produto> repository)
    {
        Console.WriteLine("⏰ === WRITE-BEHIND PATTERN ===");
        var service = new WriteBehindService(cache, repository);

        // Busca inicial
        await service.GetProdutoAsync(3);
        
        // Salvamento rápido no cache (assíncrono no DB)
        var produto = new Produto { Id = 3, Nome = "Teclado Mecânico Premium", Preco = 350.00m, Categoria = "Periféricos" };
        await service.SaveProdutoAsync(produto);
        
        // Dado está imediatamente disponível no cache
        await service.GetProdutoAsync(3);
        
        Console.WriteLine("⏳ Aguardando flush automático para o banco...");
        await Task.Delay(6000); // Aguarda o timer de flush
        
        // Força flush manual
        await service.ForceFlushAsync();
        
        service.Dispose();
        Console.WriteLine("✅ Write-Behind demonstrado\n");
    }

    static async Task DemonstrateReadThrough(IMemoryCache cache, IRepository<Produto> repository)
    {
        Console.WriteLine("📖 === READ-THROUGH PATTERN ===");
        var service = new ReadThroughService(cache, repository);

        // Read-through automático com GetOrCreateAsync
        await service.GetProdutoAsync(4);
        
        // Cache hit na segunda chamada
        await service.GetProdutoAsync(4);
        
        // Demonstração da versão manual
        await service.GetProdutoManualAsync(5);
        await service.GetProdutoManualAsync(5);
        
        Console.WriteLine("✅ Read-Through demonstrado\n");
    }

    static async Task DemonstrateRefreshAhead(IMemoryCache cache, IRepository<Produto> repository)
    {
        Console.WriteLine("🔄 === REFRESH-AHEAD PATTERN ===");
        var service = new RefreshAheadService(cache, repository);

        // Carrega dados críticos
        await service.PreloadCriticalDataAsync(new[] { 1, 2, 3 });
        
        // Busca normal
        await service.GetProdutoAsync(1);
        
        // Força refresh manual
        await service.ForceRefreshAsync(1);
        
        Console.WriteLine("⏳ Timer de refresh em background está ativo...");
        await Task.Delay(2000);
        
        service.Dispose();
        Console.WriteLine("✅ Refresh-Ahead demonstrado\n");
    }

    static async Task DemonstrateFullCache(IMemoryCache cache, IRepository<Produto> produtoRepo, IRepository<Usuario> usuarioRepo)
    {
        Console.WriteLine("🗄️ === FULL CACHE PATTERN ===");
        var service = new FullCacheService(cache, produtoRepo, usuarioRepo);

        // Inicialização completa do cache
        await service.InitializeAsync();
        
        // Buscas que sempre vêm do cache
        await service.GetProdutoAsync(1);
        await service.GetProdutoAsync(2);
        
        // Buscas por índices
        var eletronicos = await service.GetProdutosPorCategoriaAsync("Eletrônicos");
        Console.WriteLine($"🔍 Encontrados {eletronicos.Count} produtos eletrônicos");
        
        var produtosCaros = await service.GetProdutosPorFaixaPrecoAsync("alto");
        Console.WriteLine($"🔍 Encontrados {produtosCaros.Count} produtos na faixa alta");
        
        // Busca configuração
        var taxaImposto = await service.GetConfiguracaoAsync("taxa_imposto");
        Console.WriteLine($"⚙️ Taxa de imposto: {taxaImposto?.Valor}");
        
        service.Dispose();
        Console.WriteLine("✅ Full Cache demonstrado\n");
    }

    static async Task DemonstrateNearCache(IMemoryCache cache, IRepository<Produto> repository)
    {
        Console.WriteLine("🌐 === NEAR CACHE PATTERN ===");
        
        // Simula cache remoto com outra instância de MemoryCache
        var remoteCache = new MemoryCache(new MemoryCacheOptions());
        var service = new NearCacheService(cache, remoteCache, repository);

        // Primeira busca - vai até o banco e popula ambos os caches
        await service.GetProdutoAsync(1);
        
        // Segunda busca - hit no cache local (L1)
        await service.GetProdutoAsync(1);
        
        // Limpa cache local para simular L2 hit
        service.ClearLocalCache();
        await service.GetProdutoAsync(1); // Hit no L2, promove para L1
        
        // Atualização
        var produto = new Produto { Id = 1, Nome = "Produto Atualizado", Preco = 1000.00m, Categoria = "Eletrônicos" };
        await service.SaveProdutoAsync(produto);
        
        // Estatísticas
        var stats = service.GetStats();
        Console.WriteLine($"📊 Instância: {stats.InstanceId}, Cache local: {stats.LocalCacheSize} itens");
        
        service.Dispose();
        Console.WriteLine("✅ Near Cache demonstrado\n");
    }

    static async Task DemonstrateTieredCache(IMemoryCache cache, IRepository<Produto> repository)
    {
        Console.WriteLine("🎯 === TIERED CACHE PATTERN ===");
        
        var l2Cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TieredCacheService(cache, l2Cache, repository);

        // Aquecimento do cache
        await service.WarmUpCacheAsync(new[] { 1, 2, 3 });
        
        // Demonstração de busca em lote
        var produtos = await service.GetProdutosBatchAsync(new[] { 1, 2, 3, 4, 5 });
        Console.WriteLine($"📦 Busca em lote retornou {produtos.Count} produtos");
        
        // Várias buscas para demonstrar hits em diferentes níveis
        await service.GetProdutoAsync(1); // L1 hit
        await service.GetProdutoAsync(2); // L1 hit
        await service.GetProdutoAsync(6); // Miss em todos os níveis
        
        // Refresh manual
        await service.RefreshItemAsync(1);
        
        // Métricas finais
        var metrics = service.GetMetrics();
        Console.WriteLine($"📈 Métricas finais:");
        Console.WriteLine($"   Total de requisições: {metrics.TotalRequests}");
        Console.WriteLine($"   Taxa de hit L1: {metrics.L1HitRate:F2}%");
        Console.WriteLine($"   Taxa de hit L2: {metrics.L2HitRate:F2}%");
        Console.WriteLine($"   Taxa de hit geral: {metrics.CacheHitRate:F2}%");
        
        service.Dispose();
        Console.WriteLine("✅ Tiered Cache demonstrado\n");
    }
}
