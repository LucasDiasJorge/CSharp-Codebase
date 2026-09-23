# CLAUDE.md — DynamicProgrammingCoinChangeDemo

Console com coin change resolvido de quatro formas (recursiva, memoizada, tabulada e gulosa), instrumentadas por contagem de chamadas. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/DynamicProgrammingCoinChangeDemo/DynamicProgrammingCoinChangeDemo.csproj
dotnet run -c Release --project 10-Algorithms/DynamicProgrammingCoinChangeDemo/DynamicProgrammingCoinChangeDemo.csproj
```

Roda seis cenários e termina. Sem serviço externo.

## Estrutura interna

`Solvers/CoinChangeSolvers` tem as quatro implementações da **mesma** recorrência `minMoedas(v) = 1 + min(minMoedas(v - moeda))`. `Recursive` e `Memoized` são deliberadamente quase idênticas — a única diferença é o bloco `if (memo[remaining] is int cached)` e a escrita em `memo`. **Manter essa simetria**: é ela que ensina que memoization não é outro algoritmo. Refatorar as duas para um método comum com flag destrói a comparação.

`SolveResult` carrega `Calls`, que é o instrumento de todos os cenários. `Calls` é `long` porque a recursiva passa de um milhão a partir do valor 40.

`Tabulated` mantém `chosenCoin[]` além de `best[]` — é o que permite o cenário 4 reconstruir a combinação. Sem ele o exemplo só saberia contar.

`Impossible = -1` distingue "não há combinação" de "zero moedas" (resposta válida para valor 0). `IsPossible` encapsula a comparação; as verificações `subResult >= 0` dependem dessa convenção.

## Pontos de atenção

- TFM `net10.0`. A trilha é toda `net9.0`, mas a máquina não tem esse runtime. Pacote: `Microsoft.Extensions.Logging.Console`.
- **O cenário 2 chega a 1,26 milhão de chamadas recursivas no valor 40.** Aumentar esse limite trava o programa: o fator é de cerca de 21x a cada 10 unidades de valor. O valor 50 já passa de 26 milhões.
- O cenário 3 **não executa** a recursiva pura de propósito. Não "completar a tabela" chamando-a ali.
- **Afirmação de magnitude já corrigida uma vez:** o texto dizia "ordem de 10^20 chamadas" para o valor 2000. Extrapolando os próprios números do cenário 2 (×21 a cada 10 unidades), o valor real está na casa de 10^265 — a estimativa estava errada por centenas de ordens de grandeza. O texto atual não cita número, cita o fator medido. Ao mexer no cenário, não reintroduzir um expoente inventado.
- Os tempos do cenário 3 (1ms / 3ms) **variam entre execuções** — já saíram 3ms e 6ms para a tabulada. O README diz que a diferença é ruído. Não ajustar os números a cada run.
- O cenário 6 depende de `[1, 3, 4]` com valor 6. Esse é o menor contraexemplo em que o guloso erra; trocar as moedas provavelmente faz o guloso acertar e o cenário perder o sentido. A comparação com `[1, 5, 10, 25, 50]` logo abaixo existe para mostrar por que o bug passa nos testes — manter as duas.
- `Greedy` ordena com `OrderByDescending` a cada chamada. É LINQ, não tipo anônimo, então a variável continua explícita — sem `var` no projeto.
- **Fronteira com os vizinhos**: `BinarySearchBoundariesDemo` deriva algoritmo de invariante; aqui o ponto de partida é a recorrência. Não expandir para outros problemas de DP (mochila, LCS) — o exemplo é sobre as três abordagens sobre um problema só.
