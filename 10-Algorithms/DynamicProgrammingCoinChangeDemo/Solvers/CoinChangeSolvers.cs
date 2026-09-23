namespace DynamicProgrammingCoinChangeDemo.Solvers;

/// <summary>
/// Resultado de uma resolução, com o instrumento que interessa: quantas chamadas foram
/// feitas. É esse número que mostra a diferença entre as três abordagens.
/// </summary>
public sealed record SolveResult(int MinimumCoins, long Calls, IReadOnlyList<int> Coins)
{
    public bool IsPossible => MinimumCoins >= 0;
}

/// <summary>
/// As três formas de resolver o mesmo problema: qual o menor número de moedas que soma
/// exatamente um valor.
///
/// A relação de recorrência é a mesma nas três:
/// <c>minMoedas(v) = 1 + min(minMoedas(v - moeda))</c> para cada moeda que caiba.
/// O que muda é <b>o que se faz com os subproblemas repetidos</b>.
/// </summary>
public static class CoinChangeSolvers
{
    private const int Impossible = -1;

    /// <summary>
    /// Recursão pura. Correta e inviável: cada chamada reabre subproblemas que outros
    /// ramos já resolveram, e o número de chamadas cresce exponencialmente.
    /// </summary>
    public static SolveResult Recursive(IReadOnlyList<int> coins, int amount)
    {
        long calls = 0;

        int Solve(int remaining)
        {
            calls++;

            if (remaining == 0)
            {
                return 0;
            }

            if (remaining < 0)
            {
                return Impossible;
            }

            int best = Impossible;

            foreach (int coin in coins)
            {
                int subResult = Solve(remaining - coin);

                if (subResult >= 0 && (best == Impossible || subResult + 1 < best))
                {
                    best = subResult + 1;
                }
            }

            return best;
        }

        int result = Solve(amount);

        return new SolveResult(result, calls, []);
    }

    /// <summary>
    /// Memoization (top-down). A recursão é a mesma; a única diferença é guardar o
    /// resultado de cada subproblema. Com isso, cada valor é resolvido UMA vez, e o
    /// custo cai de exponencial para O(valor × moedas).
    /// </summary>
    public static SolveResult Memoized(IReadOnlyList<int> coins, int amount)
    {
        long calls = 0;
        int?[] memo = new int?[amount + 1];

        int Solve(int remaining)
        {
            calls++;

            if (remaining == 0)
            {
                return 0;
            }

            if (remaining < 0)
            {
                return Impossible;
            }

            // A consulta ao memo e o que corta a arvore: subproblema ja resolvido nao
            // e reaberto.
            if (memo[remaining] is int cached)
            {
                return cached;
            }

            int best = Impossible;

            foreach (int coin in coins)
            {
                int subResult = Solve(remaining - coin);

                if (subResult >= 0 && (best == Impossible || subResult + 1 < best))
                {
                    best = subResult + 1;
                }
            }

            memo[remaining] = best;

            return best;
        }

        int result = Solve(amount);

        return new SolveResult(result, calls, []);
    }

    /// <summary>
    /// Tabulation (bottom-up). Mesma recorrência, sem recursão: resolve de 1 até o
    /// valor, e cada posição só depende de posições já calculadas.
    ///
    /// Além de evitar o risco de estouro de pilha, permite reconstruir a resposta —
    /// guardando qual moeda foi escolhida em cada valor.
    /// </summary>
    public static SolveResult Tabulated(IReadOnlyList<int> coins, int amount)
    {
        long steps = 0;

        int[] best = new int[amount + 1];
        int[] chosenCoin = new int[amount + 1];

        Array.Fill(best, int.MaxValue);
        best[0] = 0;

        for (int value = 1; value <= amount; value++)
        {
            foreach (int coin in coins)
            {
                steps++;

                if (coin > value || best[value - coin] == int.MaxValue)
                {
                    continue;
                }

                int candidate = best[value - coin] + 1;

                if (candidate < best[value])
                {
                    best[value] = candidate;
                    chosenCoin[value] = coin;
                }
            }
        }

        if (best[amount] == int.MaxValue)
        {
            return new SolveResult(Impossible, steps, []);
        }

        // Reconstrucao: cada valor sabe qual moeda o levou ao otimo.
        List<int> used = new List<int>();
        int current = amount;

        while (current > 0)
        {
            used.Add(chosenCoin[current]);
            current -= chosenCoin[current];
        }

        used.Sort();
        used.Reverse();

        return new SolveResult(best[amount], steps, used);
    }

    /// <summary>
    /// Guloso: pega sempre a maior moeda que couber. Rápido, e <b>errado</b> para
    /// conjuntos de moedas em que o ótimo local não leva ao global.
    /// </summary>
    public static SolveResult Greedy(IReadOnlyList<int> coins, int amount)
    {
        long steps = 0;
        List<int> used = new List<int>();
        int remaining = amount;

        foreach (int coin in coins.OrderByDescending(value => value))
        {
            while (remaining >= coin)
            {
                steps++;
                remaining -= coin;
                used.Add(coin);
            }
        }

        return remaining == 0
            ? new SolveResult(used.Count, steps, used)
            : new SolveResult(Impossible, steps, []);
    }
}
