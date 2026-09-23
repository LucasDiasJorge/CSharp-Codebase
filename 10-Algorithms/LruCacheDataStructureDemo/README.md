# LruCacheDataStructureDemo

Console que combina dicionário e lista duplamente ligada para obter leitura, escrita e remoção em tempo constante, e mede o comportamento do LRU sob padrões de acesso diferentes.

## Visão geral

A pergunta que o LRU resolve é: quando o cache enche, quem sai? A resposta é "o menos recentemente usado" — e implementar isso em O(1) exige duas estruturas, porque **nenhuma delas resolve sozinha**.

O dicionário acha a chave em tempo constante, mas não faz ideia de quem foi usado por último. A lista ligada mantém a ordem de uso, mas achar um item nela é O(n). A combinação funciona por um detalhe: o dicionário guarda o **nó da lista**, não o valor. Com o nó em mãos, remover e reinserir na frente custa O(1), e o candidato a sair é sempre o último da lista — sem busca.

A lista precisa ser **duplamente** ligada. Remover um nó do meio em O(1) exige o ponteiro para o anterior; com lista simples seria preciso percorrer até encontrá-lo.

O que distingue LRU de uma fila FIFO é que **ler promove**. O exemplo mostra a diferença em uma sequência onde uma chave é acessada o tempo todo: o LRU a mantém, e o FIFO a descarta pela idade de inserção.

Os dois últimos cenários tratam de quando o LRU não ajuda. Sob varredura sequencial de um conjunto que não cabe no cache, ele acerta **zero** vezes — descarta exatamente o que será pedido em seguida. E a taxa de acerto sob acesso enviesado sobe de forma contínua com a capacidade, sem o platô que se costuma esperar.

## Conceitos abordados

- Dicionário para localizar e lista ligada para ordenar, combinados.
- Dicionário apontando para o nó da lista, não para o valor.
- Necessidade da lista duplamente ligada.
- Promoção na leitura, que distingue LRU de FIFO.
- Remoção do último da lista em O(1), sem busca.
- Remoção das duas estruturas, sob pena de vazamento.
- Varredura sequencial como pior caso do LRU.
- Relação entre capacidade e taxa de acerto.

## Objetivos de aprendizagem

- Implementar um LRU com as três operações em tempo constante.
- Explicar por que o dicionário guarda o nó, e não o valor.
- Reconhecer o padrão de acesso em que o LRU é a pior escolha.
- Dimensionar capacidade a partir do conjunto de trabalho real.

## Estrutura do projeto

```text
LruCacheDataStructureDemo/
|-- Demo/
|   `-- LruDemoRunner.cs
|-- Structures/
|   `-- LruCache.cs
|-- LruCacheDataStructureDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run -c Release --project 10-Algorithms/LruCacheDataStructureDemo/LruCacheDataStructureDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 10-Algorithms/LruCacheDataStructureDemo/LruCacheDataStructureDemo.csproj
```

Não exige serviço externo. Use `-c Release` para que as medições de tempo façam sentido.

## Boas práticas e pontos de atenção

- Guarde o **nó** da lista no dicionário, não o valor. Guardando o valor, promover um item exigiria procurá-lo na lista, e tudo voltaria a ser O(n) — que é justamente o que a estrutura existe para evitar.
- A lista precisa ser duplamente ligada. `LinkedList<T>` do .NET já é; uma implementação própria com lista simples não consegue remover do meio em O(1).
- Remova das **duas** estruturas ao descartar. Esquecer o dicionário deixa uma entrada apontando para um nó fora da lista — vazamento silencioso que aparece como cache que nunca mais acerta aquela chave.
- Leitura precisa promover. Sem isso, o que existe é uma fila FIFO com outro nome.
- `LinkedList<T>` não é thread-safe, e o LRU tem estado mutável em toda leitura. Um cache compartilhado precisa de sincronização — e o lock na leitura costuma ser o gargalo.
- Reconheça o pior caso. Varredura sequencial de um conjunto maior que o cache produz zero acertos: o LRU descarta exatamente o que será pedido a seguir. Políticas como LFU, ARC ou substituição aleatória se saem melhor aí.
- Dimensione pelo conjunto de trabalho, não por palpite. Capacidade igual ao conjunto quente ainda erra bastante, porque os acessos frios continuam expulsando chaves quentes.
- Cada entrada custa mais do que o valor. Há o nó da lista, a entrada do dicionário e os dois ponteiros — para valores pequenos, a sobrecarga pode superar o dado.
- Para cache de produção, prefira uma implementação pronta. `MemoryCache` e FusionCache resolvem expiração, tamanho e concorrência; implementar LRU à mão vale para entender, não para usar.

## Conteúdo complementar

Estrutura:

```text
dicionario:  chave -> no da lista            (achar em O(1))
lista:       [mais recente ... menos recente] (ordenar por uso)

TryGet(k)  -> acha o no pelo dicionario, move para a frente
Put(k, v)  -> insere na frente; se encheu, remove o ultimo
Evict()    -> remove o ultimo da lista E a entrada do dicionario
```

**1. Ler promove**

```text
apos inserir a, b, c:  [c > b > a]
apos LER "a":          [a > c > b]
```

**2. Remoção**

```text
antes de inserir "d":  [a > c > b]
depois de inserir "d": [d > a > c]
"b" ainda esta no cache? False
```

Saiu `b`, e não `a` — porque `a` tinha sido lido depois, mesmo tendo sido inserido antes.

**3. LRU contra FIFO**, na sequência `a, b, c, a, d, a, e, a` com capacidade 3:

| Política | Acertos | Erros |
|---|---|---|
| LRU | **3** | 5 |
| FIFO | 2 | 6 |

`a` é acessado o tempo todo. O LRU o mantém porque olha o uso; o FIFO o descarta pela idade de inserção.

**4. Custo constante**, 1 milhão de leituras:

| Capacidade | Tempo | Por operação |
|---|---|---|
| 1.000 | 21ms | 21 ns |
| 100.000 | 11ms | 11 ns |
| 1.000.000 | 13ms | 14 ns |

A capacidade cresceu mil vezes e o custo por operação ficou na mesma ordem de grandeza. A variação entre as linhas é ruído de medição e efeito de cache da CPU, não crescimento.

**5. O pior caso do LRU** — varredura de 1000 chaves com cache de 100, 3 voltas:

```text
0 acertos, 3000 erros, 2900 remocoes
```

Zero. A cada volta, tudo que estava no cache já havia sido descartado — e o LRU descartou exatamente o que seria pedido em seguida.

**6. Capacidade e taxa de acerto**, com 80% dos acessos em 20% das chaves (200 quentes de 1000):

| Capacidade | Taxa de acerto |
|---|---|
| 50 | 16,2% |
| 100 | 30,9% |
| 200 | 57,4% |
| 400 | 83,1% |
| 800 | 92,2% |
| 1.000 | 95,0% |

Repare que capacidade 200 — exatamente o tamanho do conjunto quente — rende só 57%. Os 20% de acessos frios continuam expulsando chaves quentes, e a taxa só se aproxima do teto quando a capacidade cobre quase todo o espaço de chaves.

Custo das operações:

| Operação | Dicionário sozinho | Lista sozinha | Combinação |
|---|---|---|---|
| Achar a chave | O(1) | O(n) | O(1) |
| Saber o menos usado | — | O(1) | O(1) |
| Promover um item | — | O(1) com o nó | O(1) |
| Remover o menos usado | — | O(1) | O(1) |

Relação com os vizinhos: `PriorityQueueDemo` cobre outra estrutura de descarte por critério. A trilha `06-Caching` trata de cache distribuído, expiração e invalidação — aqui o assunto é só a estrutura de dados que sustenta a política.

## Referências e documentação complementar

- https://learn.microsoft.com/dotnet/api/system.collections.generic.linkedlist-1
- https://learn.microsoft.com/dotnet/api/microsoft.extensions.caching.memory.memorycache
- https://en.wikipedia.org/wiki/Cache_replacement_policies
