# DynamicProgrammingCoinChangeDemo

Console que resolve o problema do troco (coin change) com recursão pura, memoization e tabulation, contando as chamadas de cada abordagem para mostrar de onde vem a diferença de custo.

## Visão geral

As três abordagens resolvem a **mesma** relação de recorrência:

```text
minMoedas(v) = 1 + min(minMoedas(v - moeda)), para cada moeda que caiba
```

O que muda entre elas não é a ideia, é o que se faz com os subproblemas repetidos. A recursão pura reabre `minMoedas(20)` toda vez que um ramo diferente chega a 20. A memoization guarda a resposta na primeira vez e devolve a mesma na segunda. A tabulation inverte a ordem: em vez de descer do valor até zero, sobe de 1 até o valor, e cada posição só depende de posições já calculadas.

O exemplo instrumenta as três com um contador de chamadas, porque é esse número que torna a diferença visível. Para o valor 30 com as moedas brasileiras, a recursão pura faz **59.156 chamadas** e a memoizada faz **151** — para chegar à mesma resposta de 2 moedas. O cenário 2 varre valores crescentes e mostra a razão subindo de 2x para mais de 6.000x.

Os dois últimos cenários tratam do que costuma ser tratado como detalhe. O caso impossível (`[5, 10]` não forma 3) precisa ser distinguido de "zero moedas", daí o `-1` em vez de `0`. E o algoritmo guloso — pegar sempre a maior moeda que couber — **acerta** com as moedas brasileiras e **erra** com `[1, 3, 4]`, que é exatamente o motivo pelo qual esse bug costuma passar pelos testes.

## Conceitos abordados

- Relação de recorrência como ponto de partida das três abordagens.
- Subproblemas sobrepostos, que é o que torna DP aplicável.
- Memoization (top-down): a mesma recursão com uma tabela de respostas.
- Tabulation (bottom-up): a mesma recorrência sem recursão nem pilha.
- Complexidade O(valor × moedas) contra crescimento exponencial.
- Reconstrução da solução, não apenas do seu custo.
- Caso impossível distinguido do caso trivial.
- Falha do algoritmo guloso em conjuntos não canônicos de moedas.

## Objetivos de aprendizagem

- Escrever a recorrência antes de escolher a implementação.
- Transformar uma recursão exponencial em memoizada com uma linha de consulta à tabela.
- Converter uma solução top-down em bottom-up e explicar o que se ganha.
- Reconstruir a combinação de moedas a partir da tabela de decisões.
- Identificar quando o guloso é suficiente e quando ele produz resposta subótima silenciosa.

## Estrutura do projeto

```text
DynamicProgrammingCoinChangeDemo/
|-- Demo/
|   `-- CoinChangeDemoRunner.cs
|-- Solvers/
|   `-- CoinChangeSolvers.cs
|-- DynamicProgrammingCoinChangeDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run -c Release --project 10-Algorithms/DynamicProgrammingCoinChangeDemo/DynamicProgrammingCoinChangeDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 10-Algorithms/DynamicProgrammingCoinChangeDemo/DynamicProgrammingCoinChangeDemo.csproj
```

Não exige serviço externo. Use `-c Release` porque o cenário 3 mede tempo. O programa roda seis cenários e termina.

## Boas práticas e pontos de atenção

- Escreva a recorrência primeiro. As três implementações saem dela; começar pelo código costuma produzir uma tabulação sem saber o que cada posição significa.
- A memoization é a recursão ingênua **mais uma consulta**. Se a versão memoizada ficou muito diferente da recursiva, provavelmente há dois algoritmos diferentes no arquivo.
- Prefira tabulation quando o valor for grande. A memoizada recursiva desce um nível de pilha por unidade de valor no pior caso — com moeda 1 e valor alto, isso é estouro de pilha.
- Prefira memoization quando **nem todos** os subproblemas forem alcançáveis. A tabulação calcula todas as posições de 1 até o valor, mesmo as que nenhuma combinação atinge.
- Guarde a decisão, não só o custo, se a resposta precisar ser exibida. Um vetor extra com a moeda escolhida em cada valor transforma "são 5 moedas" em "são 50 + 10 + 5 + 1 + 1".
- Distinga impossível de zero. `0` é resposta válida para o valor 0; usar `0` também para "não dá" produz bug de borda. Aqui, `-1` marca o impossível e `IsPossible` encapsula a comparação.
- Não use o guloso sem provar que o conjunto de moedas é canônico. Ele acerta com moedas reais de quase todos os países — e é por isso que o erro só aparece em produção, com um conjunto de denominações personalizado.
- Cuidado ao instrumentar: o contador de chamadas do exemplo é didático. Em código real ele seria ruído; aqui é o instrumento principal.
- `int` basta para os valores deste exemplo, mas a contagem de chamadas da recursiva pura é `long` — a partir do valor 40 o número já passa de um milhão.

## Conteúdo complementar

**1. As três abordagens** — moedas `[1, 5, 10, 25, 50]`, valor 30:

| Abordagem | Resposta | Chamadas/passos |
|---|---|---|
| Recursiva pura | 2 moedas | 59.156 |
| Memoizada | 2 moedas | 151 |
| Tabulada | 2 moedas | 150 |

Mesma resposta, três custos muito diferentes.

**2. O que a memoização corta**:

| Valor | Recursiva | Memoizada | Razão |
|---:|---:|---:|---:|
| 10 | 126 | 51 | 2x |
| 20 | 2.776 | 101 | 27x |
| 30 | 59.156 | 151 | 392x |
| 40 | 1.262.961 | 201 | 6.283x |

A memoizada cresce **linearmente** (51, 101, 151, 201 — 5 chamadas por unidade de valor, uma por moeda). A recursiva multiplica por cerca de 21 a cada 10 unidades.

**3. Escala** — valor 2000:

```text
memoizada: 40 moedas, 10.001 chamadas, 1ms
tabulada:  40 moedas, 10.000 passos,   3ms
recursiva pura: nao executada
```

A recursiva pura não entra nessa comparação: extrapolando o fator de 21 por 10 unidades do cenário 2, o número de chamadas para 2000 não tem utilidade prática nem cabe em `long`. As duas versões com DP são O(valor × moedas) = 10.000 operações no pior caso. A diferença de milissegundos entre elas varia entre execuções e é ruído de medição.

**4. Reconstruir a resposta**:

```text
30  = 2 moeda(s): [25 + 5]
67  = 5 moeda(s): [50 + 10 + 5 + 1 + 1]
99  = 8 moeda(s): [50 + 25 + 10 + 10 + 1 + 1 + 1 + 1]
```

A tabela guarda qual moeda levou ao ótimo em cada valor; percorrer de trás para frente devolve a combinação.

**5. Quando não há resposta** — moedas `[5, 10]`:

| Valor | Resultado |
|---:|---|
| 3 | impossível |
| 7 | impossível |
| 15 | 2 moedas [10 + 5] |

**6. O guloso** — moedas `[1, 3, 4]`, valor 6:

```text
guloso:  3 moedas [4 + 1 + 1]
otimo:   2 moedas [3 + 3]
```

O guloso pega o 4 porque é a maior moeda que cabe, e fica preso com 2 de resto. Com `[1, 5, 10, 25, 50]` e valor 30, guloso e ótimo coincidem em 2 moedas — testado com moedas reais, o guloso parece correto, e o erro só aparece com outro conjunto.

Complexidade:

| Abordagem | Tempo | Espaço |
|---|---|---|
| Recursiva pura | exponencial | O(valor) de pilha |
| Memoizada | O(valor × moedas) | O(valor) de tabela + pilha |
| Tabulada | O(valor × moedas) | O(valor) de tabela |
| Gulosa | O(valor / menor moeda) | O(1) — e resposta possivelmente errada |

Relação com os vizinhos: `BinarySearchBoundariesDemo` parte de um invariante para derivar um algoritmo; aqui o ponto de partida é a recorrência. `LruCacheDataStructureDemo` também usa uma tabela para evitar recomputar, mas por capacidade limitada e descarte, não por subproblemas.

## Referências e documentação complementar

- https://en.wikipedia.org/wiki/Change-making_problem
- https://en.wikipedia.org/wiki/Dynamic_programming
- https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/nullable-value-types
