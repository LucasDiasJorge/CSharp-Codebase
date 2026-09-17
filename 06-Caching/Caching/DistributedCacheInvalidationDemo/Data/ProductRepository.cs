using System.Collections.Concurrent;

namespace DistributedCacheInvalidationDemo.Data;

/// <summary>
/// Fonte da verdade, compartilhada por todos os nós. Representa o banco: quando alguém
/// escreve aqui, o dado mudou de fato — o problema do exemplo é fazer os caches locais
/// de cada nó ficarem sabendo disso.
/// </summary>
public sealed class ProductRepository
{
    private readonly ConcurrentDictionary<string, string> _products = new ConcurrentDictionary<string, string>
    {
        ["1"] = "Teclado mecanico - R$ 349,90",
        ["2"] = "Monitor 27 polegadas - R$ 1899,00"
    };

    private readonly ILogger<ProductRepository> _logger;
    private int _readCount;

    public ProductRepository(ILogger<ProductRepository> logger)
    {
        _logger = logger;
    }

    public int ReadCount => Volatile.Read(ref _readCount);

    public string? Read(string id)
    {
        Interlocked.Increment(ref _readCount);

        return _products.TryGetValue(id, out string? product) ? product : null;
    }

    public void Write(string id, string value)
    {
        _products[id] = value;
        _logger.LogInformation("Banco atualizado: produto {ProdutoId} = {Valor}", id, value);
    }

    public void ResetCounter() => Interlocked.Exchange(ref _readCount, 0);
}
