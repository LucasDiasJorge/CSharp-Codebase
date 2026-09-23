using System.Diagnostics;
using DynamicProgrammingCoinChangeDemo.Solvers;
using Microsoft.Extensions.Logging;

namespace DynamicProgrammingCoinChangeDemo.Demo;

/// <summary>
/// Seis cenários: as três abordagens no mesmo problema, a explosão dos subproblemas
/// repetidos, a escala, a reconstrução da resposta, o caso impossível e o guloso errado.
/// </summary>
public sealed class CoinChangeDemoRunner
{
    private static readonly int[] BrazilianCoins = [1, 5, 10, 25, 50];

    private readonly ILogger<CoinChangeDemoRunner> _logger;

    public CoinChangeDemoRunner(ILogger<CoinChangeDemoRunner> logger)
    {
        _logger = logger;
    }

    public void RunAll()
    {
        RunSameAnswer();
        RunRepeatedSubproblems();
        RunScale();
        RunReconstruction();
        RunImpossible();
        RunGreedyFails();
    }

    private void RunSameAnswer()
    {
        Section("1. As tres abordagens, o mesmo resultado");

        const int amount = 30;

        SolveResult recursive = CoinChangeSolvers.Recursive(BrazilianCoins, amount);
        SolveResult memoized = CoinChangeSolvers.Memoized(BrazilianCoins, amount);
        SolveResult tabulated = CoinChangeSolvers.Tabulated(BrazilianCoins, amount);

        _logger.LogInformation("Moedas [{Moedas}], valor {Valor}:", string.Join(", ", BrazilianCoins), amount);
        _logger.LogInformation("  recursiva:   {Moedas} moedas, {Chamadas} chamadas", recursive.MinimumCoins, recursive.Calls);
        _logger.LogInformation("  memoizada:   {Moedas} moedas, {Chamadas} chamadas", memoized.MinimumCoins, memoized.Calls);
        _logger.LogInformation("  tabulada:    {Moedas} moedas, {Passos} passos", tabulated.MinimumCoins, tabulated.Calls);

        _logger.LogInformation("Mesma resposta, tres custos muito diferentes.");
    }

    private void RunRepeatedSubproblems()
    {
        Section("2. O que a memoizacao corta: subproblemas repetidos");

        foreach (int amount in new[] { 10, 20, 30, 40 })
        {
            SolveResult recursive = CoinChangeSolvers.Recursive(BrazilianCoins, amount);
            SolveResult memoized = CoinChangeSolvers.Memoized(BrazilianCoins, amount);

            double ratio = (double)recursive.Calls / memoized.Calls;

            _logger.LogInformation(
                "  valor {Valor,-3}: recursiva {Recursiva,12:N0} chamadas | memoizada {Memo,4} | {Razao,9:N0}x mais",
                amount,
                recursive.Calls,
                memoized.Calls,
                ratio);
        }

        _logger.LogInformation(
            "A recursiva pura reabre os mesmos subvalores em ramos diferentes. Cada moeda a mais no valor multiplica a arvore.");
    }

    private void RunScale()
    {
        Section("3. Escala: onde a recursiva pura deixa de ser opcao");

        const int amount = 2_000;

        Stopwatch watch = Stopwatch.StartNew();
        SolveResult memoized = CoinChangeSolvers.Memoized(BrazilianCoins, amount);
        long memoMs = watch.ElapsedMilliseconds;

        watch.Restart();
        SolveResult tabulated = CoinChangeSolvers.Tabulated(BrazilianCoins, amount);
        long tableMs = watch.ElapsedMilliseconds;

        _logger.LogInformation("Valor {Valor} com {Moedas} moedas:", amount, BrazilianCoins.Length);
        _logger.LogInformation("  memoizada: {Resultado} moedas, {Chamadas:N0} chamadas, {Tempo}ms", memoized.MinimumCoins, memoized.Calls, memoMs);
        _logger.LogInformation("  tabulada:  {Resultado} moedas, {Passos:N0} passos, {Tempo}ms", tabulated.MinimumCoins, tabulated.Calls, tableMs);

        _logger.LogWarning(
            "  recursiva pura: nao executada. Pelos numeros do cenario 2, cada 10 unidades a mais no valor multiplicam as chamadas por cerca de 21 — de 2000 nao ha maquina nem tempo que baste.");

        _logger.LogInformation(
            "As duas versoes com DP sao O(valor x moedas) = {Operacoes:N0} operacoes no pior caso.",
            amount * BrazilianCoins.Length);
    }

    private void RunReconstruction()
    {
        Section("4. Reconstruir a resposta, nao so contar");

        foreach (int amount in new[] { 30, 67, 99 })
        {
            SolveResult tabulated = CoinChangeSolvers.Tabulated(BrazilianCoins, amount);

            _logger.LogInformation(
                "  {Valor,-3} = {Moedas} moeda(s): [{Lista}]",
                amount,
                tabulated.MinimumCoins,
                string.Join(" + ", tabulated.Coins));
        }

        _logger.LogInformation(
            "A tabela guarda qual moeda levou ao otimo em cada valor; percorrer de tras para frente devolve a combinacao.");
    }

    private void RunImpossible()
    {
        Section("5. Quando nao ha resposta");

        int[] coins = [5, 10];

        foreach (int amount in new[] { 3, 7, 15 })
        {
            SolveResult result = CoinChangeSolvers.Tabulated(coins, amount);

            _logger.LogInformation(
                "  moedas [5, 10], valor {Valor}: {Resultado}",
                amount,
                result.IsPossible ? $"{result.MinimumCoins} moeda(s) [{string.Join(" + ", result.Coins)}]" : "impossivel");
        }

        _logger.LogInformation("Distinguir \"impossivel\" de \"zero moedas\" e parte do problema — por isso o -1 em vez de 0.");
    }

    private void RunGreedyFails()
    {
        Section("6. O guloso: rapido e errado");

        int[] trickyCoins = [1, 3, 4];
        const int amount = 6;

        SolveResult greedy = CoinChangeSolvers.Greedy(trickyCoins, amount);
        SolveResult optimal = CoinChangeSolvers.Tabulated(trickyCoins, amount);

        _logger.LogInformation("Moedas [{Moedas}], valor {Valor}:", string.Join(", ", trickyCoins), amount);
        _logger.LogWarning("  guloso:  {Moedas} moedas [{Lista}]", greedy.MinimumCoins, string.Join(" + ", greedy.Coins));
        _logger.LogInformation("  otimo:   {Moedas} moedas [{Lista}]", optimal.MinimumCoins, string.Join(" + ", optimal.Coins));

        // Com as moedas brasileiras o guloso acerta — e e por isso que o bug passa.
        SolveResult greedyBrl = CoinChangeSolvers.Greedy(BrazilianCoins, 30);
        SolveResult optimalBrl = CoinChangeSolvers.Tabulated(BrazilianCoins, 30);

        _logger.LogInformation(
            "  com [1, 5, 10, 25, 50] e valor 30, guloso={Guloso} e otimo={Otimo} — coincidem.",
            greedyBrl.MinimumCoins,
            optimalBrl.MinimumCoins);

        _logger.LogInformation(
            "O guloso so e correto para conjuntos canonicos de moedas. Testado com moedas reais, ele parece certo; o erro aparece com outro conjunto.");
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
