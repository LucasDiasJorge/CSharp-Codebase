# TrieAutocompleteDemo

Console que implementa uma trie para autocomplete, mede o custo por comprimento da chave e mostra — com números — quando ela compensa e quando não.

## Visão geral

Uma trie é uma árvore em que cada nó representa um **prefixo**, e o caminho da raiz até o nó é que forma o texto. Disso decorrem as duas propriedades que definem a estrutura.

A primeira: o custo de buscar depende do **comprimento da chave**, não de quantas chaves existem. Procurar uma palavra de 13 letras custa 13 passos, tenha a trie mil ou cem mil palavras — e o exemplo mede exatamente isso, crescendo a trie 100 vezes sem que o custo da busca mude.

A segunda: prefixos compartilhados são armazenados uma vez só. Cinco palavras começando por "intern" ocupam 27 nós; cinco palavras sem nada em comum, e ainda mais curtas, ocupam 45.

O que a trie faz e um dicionário não é responder por prefixo. Um `HashSet` encontra chave exata em O(1), mas para achar tudo que começa com "cas" ele precisa varrer todas as chaves. A trie desce direto ao nó do prefixo e coleta a subárvore.

O último cenário é o contraponto honesto. Com 50.000 GUIDs — chaves longas e sem prefixo comum —, a trie gasta cerca de 1,4 milhão de nós e faz buscas exatas **85 vezes mais devagar** que um `HashSet`. Sem prefixo compartilhado e sem consulta por prefixo, a estrutura só acrescenta indireção.

## Conceitos abordados

- Nó como prefixo, e o caminho como chave.
- Custo O(m) por comprimento, independente da quantidade.
- Consulta por prefixo, que o dicionário não oferece.
- Contador por nó para contar sugestões em O(m).
- Compartilhamento de prefixo e economia de memória.
- Coleta ordenada da subárvore para o autocomplete.
- Custo de memória: um nó por caractere no pior caso.
- Quando o dicionário é a escolha certa.

## Objetivos de aprendizagem

- Implementar inserção, busca e autocomplete em uma trie.
- Reconhecer que o custo da trie não cresce com o número de chaves.
- Avaliar o preço em memória antes de adotar a estrutura.
- Decidir entre trie e dicionário a partir do tipo de consulta.

## Estrutura do projeto

```text
TrieAutocompleteDemo/
|-- Demo/
|   `-- TrieDemoRunner.cs
|-- Structures/
|   `-- Trie.cs
|-- Program.cs
|-- README.md
`-- TrieAutocompleteDemo.csproj
```

## Como executar

```bash
dotnet run -c Release --project 10-Algorithms/TrieAutocompleteDemo/TrieAutocompleteDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 10-Algorithms/TrieAutocompleteDemo/TrieAutocompleteDemo.csproj
```

Não exige serviço externo. Use `-c Release` para que os tempos do cenário 6 façam sentido.

## Boas práticas e pontos de atenção

- Use trie quando a consulta for por prefixo. Se todas as buscas são por chave exata, um dicionário é mais rápido e muito mais econômico.
- Mantenha um contador por nó se precisar contar sugestões. Sem ele, responder "quantas palavras começam com X" exige percorrer a subárvore inteira.
- Ordene as chaves filhas ao coletar, se a ordem importar. O dicionário interno não garante ordem nenhuma, e sugestões fora de ordem alfabética são notadas na hora.
- Limite o número de sugestões. Um prefixo curto pode corresponder a milhares de palavras, e a coleta é proporcional ao que se devolve.
- Meça a memória antes de adotar. Cada nó carrega um `Dictionary`, que é caro; no pior caso há um nó por caractere de todas as chaves.
- Para alfabeto pequeno e fixo, um array de filhos bate o dicionário. Com 26 letras, `TrieNode[26]` é mais rápido e mais compacto que `Dictionary<char, TrieNode>`.
- Considere estruturas comprimidas quando a memória apertar. Radix tree (ou Patricia trie) funde cadeias de nós com um único filho, e é o que se usa quando o número de chaves é grande.
- Normalize as chaves na inserção e na busca. Acento e maiúscula tratados de formas diferentes produzem prefixos que não casam com o que o usuário digita.

## Conteúdo complementar

Autocomplete, em uma trie de 13 palavras e 33 nós:

```text
"cas"  -> [casa, casaco, casal, casamento, caso]
"cart" -> [carta, cartao, cartaz]
"port" -> [porta, portao, porto, portugues]
"z"    -> [nenhuma]
```

Contagem por prefixo — custo igual ao comprimento do prefixo:

| Prefixo | Palavras | Passos |
|---|---|---|
| `c` | 9 | 1 |
| `cas` | 5 | 3 |
| `car` | 4 | 3 |
| `port` | 4 | 4 |

O que o dicionário não faz:

```text
busca exata "carta":      HashSet=True    trie=True
prefixo "cas" pelo HashSet: 5 resultados, varrendo as 13 chaves
prefixo "cas" pela trie:    5 resultados, em 13 passos
```

Para chave exata, os dois resolvem. Para prefixo, o `HashSet` precisa varrer tudo — e a varredura cresce com o número de chaves, enquanto a trie não.

O custo depende do comprimento, não da quantidade:

| Palavras na trie | Chave buscada | Passos |
|---|---|---|
| 1.000 | `palavra000042` (13 letras) | 13 |
| 10.000 | idem | 13 |
| 100.000 | idem | 13 |

A trie cresceu cem vezes e a busca custou o mesmo.

O preço em memória:

| Conjunto | Caracteres | Nós |
|---|---|---|
| 5 palavras com prefixo comum (`intern...`) | 65 | **27** |
| 5 palavras sem prefixo comum | 44 | **45** |

O primeiro conjunto tem mais caracteres e menos nós: o prefixo compartilhado é armazenado uma vez. O segundo se aproxima de um nó por caractere, que é o pior caso.

Quando não usar — 50.000 GUIDs de 32 caracteres, sem prefixo comum:

```text
nos alocados pela trie: ~1.400.000
500.000 buscas de uma chave existente:
  HashSet     3ms
  trie      257ms
```

Cerca de 85 vezes mais lento, com mais de um milhão de nós a mais. O número exato de nós varia entre execuções, porque as chaves são aleatórias.

Trie contra dicionário:

| Operação | `HashSet`/`Dictionary` | Trie |
|---|---|---|
| Busca exata | O(1) | O(m) |
| Existe prefixo? | O(n) — varre tudo | O(m) |
| Listar por prefixo | O(n) | O(m + tamanho da saída) |
| Contar por prefixo | O(n) | O(m) |
| Memória | Uma entrada por chave | Até um nó por caractere |
| Ordem alfabética | Não | Sim, percorrendo em ordem |

Relação com os vizinhos da trilha: `BinarySearchBoundariesDemo` resolve prefixo de outra forma — em um vetor ordenado, `lowerBound` do prefixo delimita o intervalo, sem estrutura extra. `PriorityQueueDemo` e `GraphTraversalDemo` cobrem outras estruturas.

## Referências e documentação complementar

- https://en.wikipedia.org/wiki/Trie
- https://en.wikipedia.org/wiki/Radix_tree
- https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2
