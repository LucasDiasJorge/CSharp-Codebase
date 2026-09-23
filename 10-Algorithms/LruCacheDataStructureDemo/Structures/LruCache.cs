namespace LruCacheDataStructureDemo.Structures;

/// <summary>
/// Cache LRU com leitura, escrita e remoção em tempo constante.
///
/// A ideia central é que <b>nenhuma das duas estruturas sozinha resolve</b>. O
/// dicionário acha a chave em O(1) mas não sabe quem foi usado por último. A lista
/// ligada mantém a ordem de uso mas achar um item nela é O(n). Combinando as duas — o
/// dicionário guardando o <i>nó</i> da lista, não o valor — as duas operações ficam O(1).
/// </summary>
public sealed class LruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly int _capacity;

    /// <summary>
    /// Chave para o NÓ da lista. Guardar o nó é o truque: é o que permite remover e
    /// reinserir em O(1). Guardando só o valor, mover o item para a frente exigiria
    /// procurá-lo na lista, e tudo voltaria a ser O(n).
    /// </summary>
    private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _index;

    /// <summary>
    /// Ordem de uso: a frente é o mais recente, o fim é o candidato a sair.
    /// <see cref="LinkedList{T}"/> do .NET é duplamente ligada, que é o requisito —
    /// remover um nó do meio em O(1) precisa do ponteiro para o anterior.
    /// </summary>
    private readonly LinkedList<CacheEntry> _usageOrder = new LinkedList<CacheEntry>();

    public LruCache(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacidade precisa ser positiva.");
        }

        _capacity = capacity;
        _index = new Dictionary<TKey, LinkedListNode<CacheEntry>>(capacity);
    }

    public int Count => _index.Count;

    public int Hits { get; private set; }

    public int Misses { get; private set; }

    public int Evictions { get; private set; }

    /// <summary>Chaves da mais recentemente usada para a menos.</summary>
    public IReadOnlyList<TKey> KeysByRecency
    {
        get
        {
            List<TKey> keys = new List<TKey>(_usageOrder.Count);
            foreach (CacheEntry entry in _usageOrder)
            {
                keys.Add(entry.Key);
            }

            return keys;
        }
    }

    /// <summary>
    /// Leitura. Acerto move a entrada para a frente — é o "recently used" do nome, e é
    /// o que diferencia LRU de uma fila FIFO simples.
    /// </summary>
    public bool TryGet(TKey key, out TValue? value)
    {
        if (!_index.TryGetValue(key, out LinkedListNode<CacheEntry>? node))
        {
            Misses++;
            value = default;

            return false;
        }

        Hits++;

        // Remove e reinsere na frente: as duas operacoes sao O(1) porque ja temos o no.
        _usageOrder.Remove(node);
        _usageOrder.AddFirst(node);

        value = node.Value.Value;

        return true;
    }

    /// <summary>
    /// Escrita. Chave existente vira atualização e promoção; chave nova pode provocar
    /// a remoção da menos recentemente usada.
    /// </summary>
    public void Put(TKey key, TValue value)
    {
        if (_index.TryGetValue(key, out LinkedListNode<CacheEntry>? existing))
        {
            existing.Value = new CacheEntry(key, value);
            _usageOrder.Remove(existing);
            _usageOrder.AddFirst(existing);

            return;
        }

        if (_index.Count >= _capacity)
        {
            Evict();
        }

        LinkedListNode<CacheEntry> node = _usageOrder.AddFirst(new CacheEntry(key, value));
        _index[key] = node;
    }

    public bool Remove(TKey key)
    {
        if (!_index.TryGetValue(key, out LinkedListNode<CacheEntry>? node))
        {
            return false;
        }

        _usageOrder.Remove(node);
        _index.Remove(key);

        return true;
    }

    public void ResetCounters()
    {
        Hits = 0;
        Misses = 0;
        Evictions = 0;
    }

    /// <summary>
    /// Remove o menos recentemente usado. O último da lista é sempre esse, então não há
    /// busca — e é por isso que a remoção também é O(1).
    /// </summary>
    private void Evict()
    {
        LinkedListNode<CacheEntry>? last = _usageOrder.Last;

        if (last is null)
        {
            return;
        }

        // Remover dos DOIS lugares. Esquecer o dicionario deixa uma entrada apontando
        // para um no que nao esta mais na lista — vazamento silencioso que so aparece
        // como cache que nunca mais acerta aquela chave.
        _usageOrder.RemoveLast();
        _index.Remove(last.Value.Key);

        Evictions++;
    }

    private readonly record struct CacheEntry(TKey Key, TValue Value);
}
