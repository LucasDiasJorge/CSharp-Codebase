# BinarySearchBoundariesDemo

Console que deriva busca binária a partir de um invariante e implementa as três variantes — busca exata, lower bound e upper bound — com verificação exaustiva contra busca linear.

## Visão geral

Quase todo mundo sabe o que busca binária faz, e quase ninguém acerta de primeira onde vai o `+1`. O problema não é o algoritmo: é que as decisões de fronteira costumam ser adivinhadas em vez de derivadas.

O exemplo parte de um invariante único e mantém o mesmo em todas as variantes: o intervalo é **semiaberto**, `[lo, hi)`. `lo` é o primeiro índice que ainda pode ser resposta; `hi` é o primeiro que já se sabe que não é. Disso saem sozinhas as três decisões que costumam dar errado — a condição é `lo < hi`, o descarte à esquerda é `lo = mid + 1`, e o descarte à direita é `hi = mid` (não `mid - 1`, que jogaria fora a própria resposta).

As duas fronteiras valem mais do que a busca exata. `lowerBound` devolve o primeiro elemento `>= alvo` e, quando o alvo não existe, devolve exatamente onde ele deveria ser inserido. `upperBound` devolve o primeiro `> alvo`. Juntas, delimitam o intervalo de todas as ocorrências — e contar duplicatas passa a ser uma subtração.

A correção é verificada por força bruta: todos os vetores ordenados de tamanho 0 a 6 com valores 0 a 3, contra todos os alvos de −1 a 4, comparando com uma busca linear ingênua. São 1260 casos, e é o tipo de varredura que pega qualquer erro de fronteira.

## Conceitos abordados

- Invariante do intervalo semiaberto `[lo, hi)`.
- Derivação de `lo < hi`, `lo = mid + 1` e `hi = mid`.
- Busca exata, lower bound e upper bound.
- Contagem de ocorrências a partir das fronteiras.
- Ponto de inserção e comparação com `Array.BinarySearch`.
- Overflow em `(lo + hi) / 2`.
- Custo logarítmico medido em comparações.
- Verificação exaustiva contra implementação ingênua.

## Objetivos de aprendizagem

- Derivar as fronteiras em vez de decorá-las.
- Escolher entre busca exata e fronteira conforme a pergunta.
- Reconhecer o overflow do ponto médio e por que ele sobrevive a testes.
- Validar um algoritmo de índice por varredura exaustiva.

## Estrutura do projeto

```text
BinarySearchBoundariesDemo/
|-- Demo/
|   `-- BinarySearchDemoRunner.cs
|-- Search/
|   `-- BinarySearches.cs
|-- BinarySearchBoundariesDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 10-Algorithms/BinarySearchBoundariesDemo/BinarySearchBoundariesDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 10-Algorithms/BinarySearchBoundariesDemo/BinarySearchBoundariesDemo.csproj
```

Não exige serviço externo. Roda seis cenários e termina.

## Boas práticas e pontos de atenção

- Escolha um invariante e mantenha. Meio aberto `[lo, hi)` ou fechado `[lo, hi]` — os dois funcionam, mas misturá-los é a origem da maioria dos erros de `+1`.
- Com o intervalo semiaberto, `hi = mid` (nunca `mid - 1`) quando `sorted[mid]` ainda pode ser a resposta. Usar `mid - 1` aqui descarta o próprio elemento procurado.
- Use `lo + (hi - lo) / 2`. `(lo + hi) / 2` estoura quando a soma passa de `int.MaxValue`, e o resultado negativo vira índice inválido.
- Prefira as fronteiras à busca exata quando houver duplicatas. A busca exata devolve *alguma* ocorrência, sem promessa de qual.
- `Array.BinarySearch` e `List.BinarySearch` devolvem o complemento (`~posição`) quando não encontram. É o mesmo ponto de inserção do `lowerBound`, com uma convenção a mais para lembrar.
- O vetor precisa estar ordenado pela **mesma** comparação usada na busca. Ordenar por um critério e buscar por outro produz resultado errado sem erro nenhum.
- Verifique por força bruta. Um algoritmo de índice tem pouquíssimos estados interessantes — vetor vazio, um elemento, alvo antes do primeiro, depois do último, no meio de duplicatas —, e uma varredura exaustiva cobre todos.
- `while (lo < hi)` com intervalo semiaberto termina sempre. Com `lo <= hi` e `hi = mid`, o laço pode não encolher e travar.

## Conteúdo complementar

As três variantes, no vetor `[10, 20, 30, 40, 50]`:

| Alvo | Exata | `lowerBound` | `upperBound` |
|---|---|---|---|
| 30 | índice 2 | 2 | 3 |
| 35 | não encontrado | 3 | 3 |

Quando o alvo não existe, as duas fronteiras coincidem — e o valor comum é o ponto de inserção.

Com duplicatas, em `[1, 3, 3, 3, 3, 7, 9]` e alvo 3:

```text
exata       -> indice 3   (uma ocorrencia qualquer)
lowerBound  -> 1
upperBound  -> 5
[1, 5)      -> 4 ocorrencias, em 6 comparacoes
```

Contar duplicatas é `upperBound - lowerBound`. A busca exata sozinha não daria isso.

Ponto de inserção:

```text
inserir 5  na posicao 0 -> [5, 10, 20, 30, 40, 50]
inserir 25 na posicao 2 -> [10, 20, 25, 30, 40, 50]
inserir 55 na posicao 5 -> [10, 20, 30, 40, 50, 55]
```

Verificação exaustiva:

```text
1260 casos verificados, nenhuma divergencia da busca linear
```

Todos os vetores ordenados de tamanho 0 a 6 com valores de 0 a 3, contra alvos de −1 a 4.

Custo medido, em comparações:

| n | Comparações (pior caso) | ⌈log₂(n+1)⌉ |
|---|---|---|
| 1.000 | 9 | 10 |
| 1.000.000 | 19 | 20 |
| 1.000.000.000 | — | 30 |

Multiplicar o tamanho por mil acrescenta cerca de dez comparações.

A armadilha do ponto médio:

```text
(lo + hi) / 2        -> -3            (a soma estourou)
lo + (hi - lo) / 2   -> 2147483645    (correto)
```

Com `lo = int.MaxValue - 4` e `hi = int.MaxValue`. Em vetores pequenos os dois coincidem, e é por isso que o bug atravessa testes — ele só aparece com índices grandes.

Qual variante usar:

| Pergunta | Variante |
|---|---|
| Existe? Onde está? | Exata |
| Onde inserir mantendo a ordem? | `lowerBound` |
| Primeiro elemento `>= x` | `lowerBound` |
| Primeiro elemento `> x` | `upperBound` |
| Quantos iguais a x? | `upperBound - lowerBound` |
| Todos entre a e b | `[lowerBound(a), lowerBound(b))` |

Relação com os vizinhos da trilha: `Two-Sum` e `SlidingWindows` cobrem outras técnicas sobre vetores; `PriorityQueueDemo` trata de estrutura para extrair mínimos. Aqui o assunto é exclusivamente a disciplina de fronteiras na busca binária.

## Referências e documentação complementar

- https://learn.microsoft.com/dotnet/api/system.array.binarysearch
- https://research.google/blog/extra-extra-read-all-about-it-nearly-all-binary-searches-and-mergesorts-are-broken/
- https://en.cppreference.com/w/cpp/algorithm/lower_bound
