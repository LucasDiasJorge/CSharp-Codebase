# CLAUDE.md — BinarySearchBoundariesDemo

Console com busca exata, lower bound e upper bound derivados do invariante `[lo, hi)`, com verificação exaustiva. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/BinarySearchBoundariesDemo/BinarySearchBoundariesDemo.csproj
dotnet run --project 10-Algorithms/BinarySearchBoundariesDemo/BinarySearchBoundariesDemo.csproj
```

Roda seis cenários e termina. Sem serviço externo.

## Estrutura interna

`Search/BinarySearches` tem as três variantes, **todas com o mesmo invariante semiaberto `[lo, hi)`**. Isso não é estilo: é o que torna as decisões de fronteira deriváveis em vez de decoradas. Misturar convenções entre os métodos (um fechado, outro aberto) destrói o valor didático e reintroduz os erros de `+1`.

Os três pontos que **não** podem mudar:

- `while (low < high)` — com intervalo semiaberto, termina sempre.
- `low = mid + 1` ao descartar à esquerda.
- `high = mid` (nunca `mid - 1`) ao descartar à direita — `mid` ainda pode ser a resposta no lower bound.

`Midpoint` usa `low + ((high - low) / 2)`. O cenário 6 mostra o overflow da forma ingênua; não "simplificar" de volta.

`ComparisonCounter` mede o custo. É o que transforma O(log n) em número no cenário 5.

`Demo/BinarySearchDemoRunner.RunExhaustiveCheck` gera **todos** os vetores ordenados de tamanho 0..6 com valores 0..3 e testa contra busca linear: 1260 casos. É a rede de segurança do projeto — se alguém mexer nas fronteiras e quebrar algo, este cenário acusa.

## Pontos de atenção

- TFM `net10.0`. A trilha é toda `net9.0`, mas a máquina não tem esse runtime. Pacote: `Microsoft.Extensions.Logging.Console`.
- O cenário 5 aloca um vetor de **1 milhão** de ints (~4 MB). O caso de 1 bilhão é apenas calculado, não alocado — e o log diz isso explicitamente.
- O cenário 6 usa `unchecked` de propósito para o overflow acontecer em vez de lançar. Sem isso, em contexto checked, o comportamento seria outro.
- Os números do README (1260 casos, 9/19/30 comparações, `-3` no overflow) vêm da execução. Alterar os limites do gerador exaustivo ou os tamanhos do cenário 5 invalida as tabelas.
- `GenerateSortedArrays` usa uma função local iteradora recursiva. Aumentar `length` ou `maxValue` faz o número de casos explodir combinatoriamente — 6 e 3 já dão 1260 casos com alvos.
- **Fronteira com os vizinhos**: `Two-Sum` e `SlidingWindows` cobrem outras técnicas sobre vetores ordenados/janelas; `PriorityQueueDemo` trata de heap. Aqui o assunto é só a disciplina de fronteiras — não expandir para ordenação ou outras buscas.
