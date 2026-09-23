using System.Diagnostics;
using LruCacheDataStructureDemo.Structures;
using Microsoft.Extensions.Logging;

namespace LruCacheDataStructureDemo.Demo;

/// <summary>
/// Seis cenários: a ordem de uso, a remoção, LRU contra FIFO, o custo constante
/// medido, o padrão de acesso que derruba o LRU e o efeito da capacidade.
/// </summary>
public sealed class LruDemoRunner
{
    private readonly ILogger<LruDemoRunner> _logger;

    public LruDemoRunner(ILogger<LruDemoRunner> logger)
    {
        _logger = logger;
    }

    public void RunAll()
    {
        RunUsageOrder();
        RunEviction();
        RunVersusFifo();
        RunConstantCost();
        RunSequentialScan();
        RunCapacityEffect();
    }

    private void RunUsageOrder()
    {
        Section("1. Ler promove: a ordem e de USO, nao de insercao");

        LruCache<string, int> cache = new LruCache<string, int>(capacity: 3);

        cache.Put("a", 1);
        cache.Put("b", 2);
        cache.Put("c", 3);

        _logger.LogInformation("  apos inserir a, b, c:  [{Ordem}]", string.Join(" > ", cache.KeysByRecency));

        cache.TryGet("a", out _);

        _logger.LogInformation("  apos LER \"a\":          [{Ordem}] — a leitura moveu 'a' para a frente", string.Join(" > ", cache.KeysByRecency));
    }

    private void RunEviction()
    {
        Section("2. Remocao: sai o menos recentemente usado");

        LruCache<string, int> cache = new LruCache<string, int>(capacity: 3);

        cache.Put("a", 1);
        cache.Put("b", 2);
        cache.Put("c", 3);
        cache.TryGet("a", out _);

        _logger.LogInformation("  antes de inserir \"d\":  [{Ordem}]", string.Join(" > ", cache.KeysByRecency));

        cache.Put("d", 4);

        _logger.LogInformation("  depois de inserir \"d\": [{Ordem}]", string.Join(" > ", cache.KeysByRecency));

        bool stillThere = cache.TryGet("b", out _);

        _logger.LogInformation(
            "  \"b\" ainda esta no cache? {Resposta}. Saiu 'b' e nao 'a', porque 'a' tinha sido lido depois.",
            stillThere);
    }

    private void RunVersusFifo()
    {
        Section("3. LRU contra FIFO: a diferenca aparece no reacesso");

        string[] accesses = ["a", "b", "c", "a", "d", "a", "e", "a"];

        LruCache<string, int> lru = new LruCache<string, int>(capacity: 3);
        Queue<string> fifoOrder = new Queue<string>();
        HashSet<string> fifoSet = new HashSet<string>();
        int fifoHits = 0;
        int fifoMisses = 0;

        foreach (string key in accesses)
        {
            // LRU
            if (!lru.TryGet(key, out _))
            {
                lru.Put(key, key.GetHashCode());
            }

            // FIFO: sai sempre o mais antigo INSERIDO, ignorando o uso.
            if (fifoSet.Contains(key))
            {
                fifoHits++;
            }
            else
            {
                fifoMisses++;

                if (fifoSet.Count >= 3)
                {
                    string oldest = fifoOrder.Dequeue();
                    fifoSet.Remove(oldest);
                }

                fifoOrder.Enqueue(key);
                fifoSet.Add(key);
            }
        }

        _logger.LogInformation("  sequencia: {Sequencia}", string.Join(", ", accesses));
        _logger.LogInformation("  LRU  (cap 3): {Acertos} acertos, {Erros} erros", lru.Hits, lru.Misses);
        _logger.LogInformation("  FIFO (cap 3): {Acertos} acertos, {Erros} erros", fifoHits, fifoMisses);

        _logger.LogInformation(
            "\"a\" e acessado o tempo todo. O LRU o mantem porque olha o USO; o FIFO o descarta pela idade de insercao.");
    }

    private void RunConstantCost()
    {
        Section("4. Custo constante: o tempo por operacao nao cresce com a capacidade");

        foreach (int capacity in new[] { 1_000, 100_000, 1_000_000 })
        {
            LruCache<int, int> cache = new LruCache<int, int>(capacity);

            // Enche ate a capacidade.
            for (int index = 0; index < capacity; index++)
            {
                cache.Put(index, index);
            }

            const int operations = 1_000_000;
            Stopwatch watch = Stopwatch.StartNew();

            for (int index = 0; index < operations; index++)
            {
                cache.TryGet(index % capacity, out _);
            }

            watch.Stop();

            double nanosPerOp = watch.Elapsed.TotalMilliseconds * 1_000_000 / operations;

            _logger.LogInformation(
                "  capacidade {Capacidade,-9}: {Operacoes} leituras em {Tempo}ms ({Nanos:F0} ns por operacao)",
                capacity,
                operations,
                watch.ElapsedMilliseconds,
                nanosPerOp);
        }

        _logger.LogInformation("A capacidade cresceu mil vezes e o custo por operacao ficou na mesma ordem de grandeza.");
    }

    private void RunSequentialScan()
    {
        Section("5. O padrao que derruba o LRU: varredura sequencial");

        LruCache<int, int> cache = new LruCache<int, int>(capacity: 100);

        // Varre 1000 chaves distintas, em ciclo. A cada volta, tudo o que esta no cache
        // ja foi descartado — e o cache erra sempre.
        for (int round = 0; round < 3; round++)
        {
            for (int key = 0; key < 1_000; key++)
            {
                if (!cache.TryGet(key, out _))
                {
                    cache.Put(key, key);
                }
            }
        }

        _logger.LogWarning(
            "  varredura de 1000 chaves com cache de 100, 3 voltas: {Acertos} acertos, {Erros} erros, {Remocoes} remocoes.",
            cache.Hits,
            cache.Misses,
            cache.Evictions);

        _logger.LogInformation(
            "Zero acertos. O conjunto de trabalho nao cabe, e o LRU descarta exatamente o que sera pedido em seguida — aqui um cache aleatorio ou LFU se sairia melhor.");
    }

    private void RunCapacityEffect()
    {
        Section("6. Efeito da capacidade sobre a taxa de acerto");

        // Acesso enviesado: 80% das leituras caem em 20% das chaves.
        Random random = new Random(20260922);
        int[] accesses = new int[20_000];

        for (int index = 0; index < accesses.Length; index++)
        {
            accesses[index] = random.NextDouble() < 0.8
                ? random.Next(0, 200)
                : random.Next(200, 1_000);
        }

        foreach (int capacity in new[] { 50, 100, 200, 400, 800, 1_000 })
        {
            LruCache<int, int> cache = new LruCache<int, int>(capacity);

            foreach (int key in accesses)
            {
                if (!cache.TryGet(key, out _))
                {
                    cache.Put(key, key);
                }
            }

            double hitRate = 100.0 * cache.Hits / (cache.Hits + cache.Misses);

            _logger.LogInformation(
                "  capacidade {Capacidade,-4}: {Taxa:F1}% de acerto ({Acertos} acertos, {Erros} erros)",
                capacity,
                hitRate,
                cache.Hits,
                cache.Misses);
        }

        _logger.LogInformation(
            "A taxa sobe de forma continua: mesmo com capacidade igual ao conjunto quente (200), os 20% de acessos frios continuam expulsando chaves quentes. So perto de cobrir o espaco inteiro (1000) o cache para de errar.");
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
