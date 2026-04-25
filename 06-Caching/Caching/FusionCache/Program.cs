using ZiggyCreatures.Caching.Fusion; // Certifique-se de que este using está presente
// Adicione a referência ao pacote NuGet ZiggyCreatures.FusionCache em seu projeto:
// No terminal do Visual Studio, execute:
// dotnet add package ZiggyCreatures.FusionCache

// O restante do código permanece igual.

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Criar instância do FusionCache
        var cache = new FusionCache(new FusionCacheOptions());

        Console.WriteLine("=== Demo FusionCache ===\n");

        // 2. Exemplo básico - Set e Get
        Console.WriteLine("1. Set e Get básicos:");
        cache.Set("minha-chave", "Olá FusionCache!", TimeSpan.FromMinutes(5));
        var valor = cache.GetOrDefault<string>("minha-chave");
        Console.WriteLine($"Valor do cache: {valor}\n");

        // 3. GetOrSet - busca ou cria se não existir
        Console.WriteLine("2. GetOrSet (primeira vez - vai executar factory):");
        var produto = cache.GetOrSet<string>(
            "produto:1",
            (_, _) => BuscarProdutoDoBanco(1),
            options => options.SetDuration(TimeSpan.FromMinutes(10))
        );
        Console.WriteLine($"Produto: {produto}\n");

        Console.WriteLine("3. GetOrSet (segunda vez - vem do cache):");
        produto = cache.GetOrSet<string>(
            "produto:1",
            (_, _) => BuscarProdutoDoBanco(1),
            options => options.SetDuration(TimeSpan.FromMinutes(10))
        );
        Console.WriteLine($"Produto: {produto}\n");

        // 4. GetOrSetAsync - versão assíncrona
        Console.WriteLine("4. GetOrSetAsync:");
        var usuario = await cache.GetOrSetAsync(
            "usuario:42",
            async _ => await BuscarUsuarioApiAsync(42),
            TimeSpan.FromMinutes(15)
        );
        Console.WriteLine($"Usuário: {usuario}\n");

        // 5. Remover do cache
        Console.WriteLine("5. Removendo item:");
        cache.Remove("minha-chave");
        var valorRemovido = cache.GetOrDefault<string>("minha-chave");
        Console.WriteLine($"Valor após remoção: {valorRemovido ?? "NULL"}\n");

        // 6. TryGet - verifica se existe
        Console.WriteLine("6. TryGet:");
        var produtoMaybe = cache.TryGet<string>("produto:1");
        if (produtoMaybe.HasValue)
        {
            Console.WriteLine($"Produto encontrado: {produtoMaybe.Value}");
        }
        else
        {
            Console.WriteLine("Produto não encontrado no cache");
        }

        // 7. Exemplo com objeto complexo
        Console.WriteLine("\n7. Cache de objeto complexo:");
        var pedido = new Pedido
        {
            Id = 100,
            Cliente = "João Silva",
            Total = 1500.50m,
            Data = DateTime.Now
        };

        cache.Set("pedido:100", pedido, TimeSpan.FromHours(1));
        var pedidoCache = cache.GetOrDefault<Pedido>("pedido:100");
        Console.WriteLine($"Pedido: #{pedidoCache?.Id} - {pedidoCache?.Cliente} - R$ {pedidoCache?.Total}");

        // 8. Cache com opções avançadas
        Console.WriteLine("\n8. Cache com fail-safe (tratamento de erro):");
        var config = cache.GetOrSet<string?>(
            "config:api",
            (_, _) =>
            {
                // Simula uma falha
                throw new Exception("API indisponível!");
            },
            options => options
                .SetDuration(TimeSpan.FromMinutes(30))
                .SetFailSafe(true) // Mantém valor antigo se der erro
                .SetFactoryTimeouts(TimeSpan.FromSeconds(5))
        );
        Console.WriteLine($"Config: {config ?? "Falhou, mas não quebrou o app"}");

        Console.WriteLine("\n=== Fim do Demo ===");
    }

    // Simula busca no banco de dados
    static string BuscarProdutoDoBanco(int id)
    {
        Console.WriteLine("  → Buscando produto no banco de dados...");
        Thread.Sleep(1000); // Simula latência
        return $"Produto #{id} - Notebook Dell";
    }

    // Simula chamada assíncrona à API
    static async Task<string> BuscarUsuarioApiAsync(int id)
    {
        Console.WriteLine("  → Buscando usuário na API...");
        await Task.Delay(1000); // Simula latência
        return $"Usuário #{id} - Maria Santos";
    }
}

// Classe de exemplo
public class Pedido
{
    public int Id { get; set; }
    public string Cliente { get; set; }
    public decimal Total { get; set; }
    public DateTime Data { get; set; }
}