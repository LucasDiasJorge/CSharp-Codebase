using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TrieAutocompleteDemo.Structures;

namespace TrieAutocompleteDemo.Demo;

/// <summary>
/// Seis cenários: autocomplete, o custo por comprimento, a comparação com dicionário,
/// contagem por prefixo, o custo de memória e onde a trie não compensa.
/// </summary>
public sealed class TrieDemoRunner
{
    private readonly ILogger<TrieDemoRunner> _logger;

    public TrieDemoRunner(ILogger<TrieDemoRunner> logger)
    {
        _logger = logger;
    }

    public void RunAll()
    {
        Trie small = BuildSmallTrie();

        RunAutocomplete(small);
        RunPrefixCount(small);
        RunVersusDictionary(small);
        RunCostByLength();
        RunMemoryCost();
        RunWhenNotToUse();
    }

    private Trie BuildSmallTrie()
    {
        Trie trie = new Trie();

        foreach (string word in new[]
        {
            "casa", "casaco", "casal", "casamento", "caso",
            "carro", "carta", "cartao", "cartaz",
            "porta", "portao", "porto", "portugues"
        })
        {
            trie.Insert(word);
        }

        return trie;
    }

    private void RunAutocomplete(Trie trie)
    {
        Section("1. Autocomplete: sugestoes a partir de um prefixo");

        _logger.LogInformation("Trie com {Palavras} palavras em {Nos} nos.", trie.WordCount, trie.NodeCount);

        foreach (string prefix in new[] { "cas", "cart", "port", "z" })
        {
            IReadOnlyList<string> suggestions = trie.Autocomplete(prefix, limit: 5);

            _logger.LogInformation(
                "  \"{Prefixo}\" -> [{Sugestoes}]",
                prefix,
                suggestions.Count == 0 ? "nenhuma" : string.Join(", ", suggestions));
        }
    }

    private void RunPrefixCount(Trie trie)
    {
        Section("2. Contagem por prefixo em O(m)");

        foreach (string prefix in new[] { "cas", "car", "port", "c" })
        {
            int count = trie.CountWithPrefix(prefix);

            _logger.LogInformation(
                "  \"{Prefixo}\": {Quantidade} palavra(s), em {Passos} passo(s) — o contador vive no proprio no.",
                prefix,
                count,
                trie.LastOperationSteps);
        }

        _logger.LogInformation("Sem o contador, responder isso exigiria percorrer a subarvore inteira.");
    }

    private void RunVersusDictionary(Trie trie)
    {
        Section("3. O que o dicionario nao faz");

        HashSet<string> dictionary = new HashSet<string>
        {
            "casa", "casaco", "casal", "casamento", "caso",
            "carro", "carta", "cartao", "cartaz",
            "porta", "portao", "porto", "portugues"
        };

        _logger.LogInformation("  busca exata \"carta\": HashSet={Hash}, trie={Trie}", dictionary.Contains("carta"), trie.Contains("carta"));

        // Para responder por prefixo, o HashSet precisa varrer TODAS as chaves.
        int scanned = 0;
        List<string> byScan = new List<string>();
        foreach (string word in dictionary)
        {
            scanned++;

            if (word.StartsWith("cas", StringComparison.Ordinal))
            {
                byScan.Add(word);
            }
        }

        IReadOnlyList<string> byTrie = trie.Autocomplete("cas", limit: 100);

        _logger.LogInformation("  prefixo \"cas\" pelo HashSet: {Quantidade} resultados, varrendo {Varridas} chaves", byScan.Count, scanned);
        _logger.LogInformation("  prefixo \"cas\" pela trie:    {Quantidade} resultados, em {Passos} passos", byTrie.Count, trie.LastOperationSteps);

        _logger.LogInformation(
            "O HashSet acha chave exata em O(1) e prefixo em O(n). A trie acha os dois em O(m) — e e para isso que ela existe.");
    }

    private void RunCostByLength()
    {
        Section("4. O custo depende do COMPRIMENTO, nao da quantidade");

        foreach (int size in new[] { 1_000, 10_000, 100_000 })
        {
            Trie trie = new Trie();

            for (int index = 0; index < size; index++)
            {
                trie.Insert($"palavra{index:D6}");
            }

            const string probe = "palavra000042";
            bool found = trie.Contains(probe);

            _logger.LogInformation(
                "  {Tamanho,-7} palavras: buscar \"{Chave}\" ({Comprimento} letras) custou {Passos} passos, encontrado={Achado}",
                size,
                probe,
                probe.Length,
                trie.LastOperationSteps,
                found);
        }

        _logger.LogInformation("A trie cresceu 100 vezes e o custo da busca nao mudou: ele e o comprimento da chave.");
    }

    private void RunMemoryCost()
    {
        Section("5. O preco: nos alocados");

        string[] shared = ["internacional", "internacionalizacao", "internacionalmente", "internar", "interno"];
        string[] distinct = ["abacaxi", "bicicleta", "computador", "dinossauro", "elefante"];

        Trie sharedTrie = new Trie();
        foreach (string word in shared)
        {
            sharedTrie.Insert(word);
        }

        Trie distinctTrie = new Trie();
        foreach (string word in distinct)
        {
            distinctTrie.Insert(word);
        }

        int sharedChars = shared.Sum(word => word.Length);
        int distinctChars = distinct.Sum(word => word.Length);

        _logger.LogInformation(
            "  5 palavras com prefixo comum ({Chars} chars):   {Nos} nos",
            sharedChars,
            sharedTrie.NodeCount);

        _logger.LogInformation(
            "  5 palavras sem prefixo comum ({Chars} chars):  {Nos} nos",
            distinctChars,
            distinctTrie.NodeCount);

        _logger.LogInformation(
            "Prefixo compartilhado e armazenado uma vez so. Sem compartilhamento, a trie tende a um no por caractere — e cada no carrega um Dictionary.");
    }

    private void RunWhenNotToUse()
    {
        Section("6. Quando a trie NAO compensa");

        Trie trie = new Trie();
        HashSet<string> set = new HashSet<string>();

        const int total = 50_000;
        string probe = string.Empty;

        for (int index = 0; index < total; index++)
        {
            string key = Guid.NewGuid().ToString("N");
            trie.Insert(key);
            set.Add(key);

            if (index == total / 2)
            {
                // Guarda uma chave EXISTENTE: buscar algo inexistente sairia na
                // primeira letra e nao compararia o custo real da descida.
                probe = key;
            }
        }

        const int lookups = 500_000;

        Stopwatch watch = Stopwatch.StartNew();
        for (int index = 0; index < lookups; index++)
        {
            set.Contains(probe);
        }

        long setMs = watch.ElapsedMilliseconds;

        watch.Restart();
        for (int index = 0; index < lookups; index++)
        {
            trie.Contains(probe);
        }

        long trieMs = watch.ElapsedMilliseconds;

        _logger.LogInformation(
            "  {Total} chaves de 32 caracteres sem prefixo comum: trie usou {Nos} nos.",
            total,
            trie.NodeCount);

        _logger.LogInformation("  {Buscas} buscas de uma chave existente: HashSet {SetMs}ms, trie {TrieMs}ms", lookups, setMs, trieMs);

        _logger.LogInformation(
            "Sem prefixo comum e sem consulta por prefixo, a trie so acrescenta nos e indirecao. Use dicionario.");
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
