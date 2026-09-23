namespace TrieAutocompleteDemo.Structures;

/// <summary>
/// Nó da trie. Cada nó representa um PREFIXO, não uma palavra: o caminho da raiz até
/// aqui é que forma o texto. Por isso prefixos compartilhados custam memória uma vez só.
/// </summary>
public sealed class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();

    /// <summary>Marca que o caminho até aqui forma uma palavra completa.</summary>
    public bool IsWord { get; set; }

    /// <summary>
    /// Quantas palavras existem nesta subárvore. Mantido na inserção para que contar
    /// sugestões de um prefixo seja O(1) em vez de percorrer a subárvore.
    /// </summary>
    public int WordCount { get; set; }
}

/// <summary>
/// Trie (árvore de prefixos). A propriedade que a define: o custo de buscar depende do
/// <b>comprimento da chave</b>, não da quantidade de chaves armazenadas. Procurar um
/// prefixo de 5 letras custa 5 passos, tenha a trie 100 ou 100.000 palavras.
/// </summary>
public sealed class Trie
{
    private readonly TrieNode _root = new TrieNode();

    public int WordCount => _root.WordCount;

    /// <summary>Nós alocados — a medida do custo de memória da estrutura.</summary>
    public int NodeCount { get; private set; } = 1;

    /// <summary>Passos dados na última operação, para medir o custo.</summary>
    public int LastOperationSteps { get; private set; }

    public void Insert(string word)
    {
        TrieNode current = _root;
        current.WordCount++;

        foreach (char character in word)
        {
            if (!current.Children.TryGetValue(character, out TrieNode? next))
            {
                next = new TrieNode();
                current.Children[character] = next;
                NodeCount++;
            }

            current = next;
            current.WordCount++;
        }

        current.IsWord = true;
    }

    /// <summary>
    /// Verifica se a palavra existe. O(m), com m = comprimento — e independente de
    /// quantas palavras a trie contém.
    /// </summary>
    public bool Contains(string word)
    {
        LastOperationSteps = 0;
        TrieNode? node = Descend(word);

        return node is not null && node.IsWord;
    }

    /// <summary>
    /// Existe alguma palavra com este prefixo? É a pergunta que a trie responde bem e o
    /// dicionário não: um <c>Dictionary</c> encontra chaves exatas, não prefixos.
    /// </summary>
    public bool StartsWith(string prefix)
    {
        LastOperationSteps = 0;

        return Descend(prefix) is not null;
    }

    /// <summary>Quantas palavras começam com o prefixo — O(m), pelo contador do nó.</summary>
    public int CountWithPrefix(string prefix)
    {
        LastOperationSteps = 0;
        TrieNode? node = Descend(prefix);

        return node?.WordCount ?? 0;
    }

    /// <summary>
    /// As sugestões do autocomplete. Custa O(m) para chegar ao nó do prefixo, mais o
    /// percurso da subárvore — proporcional ao que é devolvido, não ao tamanho total.
    /// </summary>
    public IReadOnlyList<string> Autocomplete(string prefix, int limit)
    {
        LastOperationSteps = 0;
        TrieNode? start = Descend(prefix);

        List<string> results = new List<string>();

        if (start is null)
        {
            return results;
        }

        Collect(start, prefix, results, limit);

        return results;
    }

    private void Collect(TrieNode node, string prefix, List<string> results, int limit)
    {
        if (results.Count >= limit)
        {
            return;
        }

        if (node.IsWord)
        {
            results.Add(prefix);
        }

        // Ordenar as chaves faz as sugestoes sairem em ordem alfabetica. Sem isso a
        // ordem seria a do dicionario interno, que nao tem garantia nenhuma.
        foreach (char character in node.Children.Keys.Order())
        {
            if (results.Count >= limit)
            {
                return;
            }

            LastOperationSteps++;
            Collect(node.Children[character], prefix + character, results, limit);
        }
    }

    private TrieNode? Descend(string text)
    {
        TrieNode current = _root;

        foreach (char character in text)
        {
            LastOperationSteps++;

            if (!current.Children.TryGetValue(character, out TrieNode? next))
            {
                return null;
            }

            current = next;
        }

        return current;
    }
}
