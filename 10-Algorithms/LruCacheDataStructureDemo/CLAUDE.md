# CLAUDE.md — LruCacheDataStructureDemo

Console com cache LRU em O(1), combinando dicionário e lista duplamente ligada. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/LruCacheDataStructureDemo/LruCacheDataStructureDemo.csproj
dotnet run -c Release --project 10-Algorithms/LruCacheDataStructureDemo/LruCacheDataStructureDemo.csproj
```

**Usar `-c Release`**: o cenário 4 mede nanossegundos por operação. Roda seis cenários e termina. Sem serviço externo.

## Estrutura interna

`Structures/LruCache` tem **uma decisão que sustenta tudo**: `_index` é `Dictionary<TKey, LinkedListNode<CacheEntry>>` — mapeia para o **nó**, não para o valor. É o que permite `_usageOrder.Remove(node)` em O(1). Trocar para `Dictionary<TKey, TValue>` parece uma simplificação e transforma toda promoção em O(n), sem quebrar nenhum teste de comportamento.

`Evict()` remove das **duas** estruturas. Esquecer `_index.Remove` deixa entrada órfã apontando para nó fora da lista — o cache passa a nunca mais acertar aquela chave, sem erro nenhum.

`TryGet` promove na leitura. Sem isso o que resta é FIFO, e o cenário 3 existe para mostrar a diferença.

`Hits`/`Misses`/`Evictions` são os instrumentos dos cenários 3, 5 e 6.

## Pontos de atenção

- TFM `net10.0`. A trilha é toda `net9.0`, mas a máquina não tem esse runtime. Pacote: `Microsoft.Extensions.Logging.Console`.
- **O cenário 4 aloca um cache de 1 milhão de entradas** e faz 3 milhões de leituras no total. Leva alguns segundos e consome memória.
- Os tempos do cenário 4 (21/11/14 ns) **não crescem monotonicamente** — a linha de capacidade 1.000 saiu mais lenta que a de 100.000. É ruído de medição e efeito de cache da CPU; o README diz isso explicitamente. Não "consertar" os números.
- **Conclusão já corrigida uma vez:** o cenário 6 afirmava que a taxa de acerto estabilizava ao cobrir o conjunto quente (200 chaves). Os dados mostram o contrário — 200 rende 57% e 400 rende 83%, porque os 20% de acessos frios seguem expulsando chaves quentes. O texto atual acompanha os números; ao mexer nas capacidades ou no viés, conferir se ele continua verdadeiro.
- O `Random` do cenário 6 tem seed fixa (`20260922`), então as taxas são reproduzíveis. Trocar a seed muda os percentuais do README.
- `LruCache` **não é thread-safe** — toda leitura muta a lista. Citado no README; não adicionar lock, porque sincronização não é o assunto aqui.
- **Fronteira com os vizinhos**: `PriorityQueueDemo` cobre outra estrutura de descarte por critério. A trilha `06-Caching` trata de cache distribuído, expiração e invalidação — aqui o assunto é só a estrutura de dados. Não expandir para TTL ou cache distribuído.
