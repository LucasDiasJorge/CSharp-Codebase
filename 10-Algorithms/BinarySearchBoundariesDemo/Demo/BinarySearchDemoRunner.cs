using BinarySearchBoundariesDemo.Search;
using Microsoft.Extensions.Logging;

namespace BinarySearchBoundariesDemo.Demo;

/// <summary>
/// Seis cenários: as três buscas, o caso com duplicatas, a verificação exaustiva contra
/// busca linear, o custo logarítmico medido e a armadilha do overflow.
/// </summary>
public sealed class BinarySearchDemoRunner
{
    private readonly ComparisonCounter _counter = new ComparisonCounter();
    private readonly ILogger<BinarySearchDemoRunner> _logger;

    public BinarySearchDemoRunner(ILogger<BinarySearchDemoRunner> logger)
    {
        _logger = logger;
    }

    public void RunAll()
    {
        RunThreeSearches();
        RunDuplicates();
        RunInsertionPoint();
        RunExhaustiveCheck();
        RunLogarithmicCost();
        RunOverflow();
    }

    private void RunThreeSearches()
    {
        Section("1. As tres buscas sobre o mesmo vetor");

        int[] sorted = [10, 20, 30, 40, 50];
        _logger.LogInformation("Vetor: [{Vetor}]", string.Join(", ", sorted));

        foreach (int target in new[] { 30, 35 })
        {
            _counter.Reset();
            int exact = BinarySearches.Exact(sorted, target, _counter);

            _counter.Reset();
            int lower = BinarySearches.LowerBound(sorted, target, _counter);

            _counter.Reset();
            int upper = BinarySearches.UpperBound(sorted, target, _counter);

            _logger.LogInformation(
                "  alvo {Alvo}: exato={Exato}, lowerBound={Lower}, upperBound={Upper}",
                target,
                exact == -1 ? "nao encontrado" : exact.ToString(),
                lower,
                upper);
        }

        _logger.LogInformation("Para 35, que nao existe, as fronteiras coincidem em 3 — a posicao onde ele entraria.");
    }

    private void RunDuplicates()
    {
        Section("2. Com duplicatas: onde a busca exata deixa a desejar");

        int[] sorted = [1, 3, 3, 3, 3, 7, 9];
        _logger.LogInformation("Vetor: [{Vetor}]", string.Join(", ", sorted));

        _counter.Reset();
        int exact = BinarySearches.Exact(sorted, 3, _counter);

        _counter.Reset();
        int lower = BinarySearches.LowerBound(sorted, 3, _counter);

        _counter.Reset();
        int upper = BinarySearches.UpperBound(sorted, 3, _counter);

        _counter.Reset();
        int occurrences = BinarySearches.CountOccurrences(sorted, 3, _counter);

        _logger.LogInformation(
            "  alvo 3: exato devolveu o indice {Exato} (uma ocorrencia qualquer); lowerBound={Lower}, upperBound={Upper}.",
            exact,
            lower,
            upper);

        _logger.LogInformation(
            "  o intervalo [{Lower}, {Upper}) contem todas as ocorrencias: contagem = {Quantidade}, em {Comparacoes} comparacoes.",
            lower,
            upper,
            occurrences,
            _counter.Count);

        _logger.LogInformation("Contar ocorrencias sai de graca das fronteiras — a busca exata sozinha nao daria isso.");
    }

    private void RunInsertionPoint()
    {
        Section("3. Ponto de insercao: lowerBound ja e a resposta");

        int[] sorted = [10, 20, 30, 40, 50];

        foreach (int value in new[] { 5, 25, 55 })
        {
            _counter.Reset();
            int position = BinarySearches.LowerBound(sorted, value, _counter);

            List<int> inserted = new List<int>(sorted);
            inserted.Insert(position, value);

            _logger.LogInformation("  inserir {Valor} na posicao {Posicao} -> [{Resultado}]", value, position, string.Join(", ", inserted));
        }

        _logger.LogInformation(
            "Array.BinarySearch resolve isso devolvendo o complemento (~posicao) quando nao acha; lowerBound devolve a posicao direto.");
    }

    private void RunExhaustiveCheck()
    {
        Section("4. Verificacao exaustiva contra busca linear");

        int failures = 0;
        int cases = 0;

        // Todos os vetores possiveis de tamanho 0 a 6 com valores 0..3, e todos os
        // alvos de -1 a 4. E forca bruta suficiente para pegar qualquer erro de
        // fronteira, que e justamente onde busca binaria costuma errar.
        for (int length = 0; length <= 6; length++)
        {
            foreach (int[] candidate in GenerateSortedArrays(length, maxValue: 3))
            {
                for (int target = -1; target <= 4; target++)
                {
                    cases++;

                    int expectedLower = LinearLowerBound(candidate, target);
                    int expectedUpper = LinearUpperBound(candidate, target);

                    _counter.Reset();
                    int actualLower = BinarySearches.LowerBound(candidate, target, _counter);

                    _counter.Reset();
                    int actualUpper = BinarySearches.UpperBound(candidate, target, _counter);

                    _counter.Reset();
                    int actualExact = BinarySearches.Exact(candidate, target, _counter);

                    bool exactOk = actualExact == -1
                        ? !candidate.Contains(target)
                        : candidate[actualExact] == target;

                    if (actualLower != expectedLower || actualUpper != expectedUpper || !exactOk)
                    {
                        failures++;

                        if (failures <= 3)
                        {
                            _logger.LogError(
                                "  FALHA em [{Vetor}] alvo {Alvo}: lower {AtualL}/{EsperadoL}, upper {AtualU}/{EsperadoU}",
                                string.Join(",", candidate),
                                target,
                                actualLower,
                                expectedLower,
                                actualUpper,
                                expectedUpper);
                        }
                    }
                }
            }
        }

        if (failures == 0)
        {
            _logger.LogInformation("{Casos} casos verificados, nenhuma divergencia da busca linear.", cases);
        }
        else
        {
            _logger.LogError("{Falhas} divergencias em {Casos} casos.", failures, cases);
        }
    }

    private void RunLogarithmicCost()
    {
        Section("5. O custo logaritmico, medido");

        foreach (int size in new[] { 1_000, 1_000_000, 1_000_000_000 })
        {
            int[] probe = size <= 1_000_000 ? BuildRange(size) : [];

            if (probe.Length > 0)
            {
                _counter.Reset();
                BinarySearches.LowerBound(probe, probe[^1] + 1, _counter);

                _logger.LogInformation(
                    "  n = {Tamanho,-10}: {Comparacoes} comparacoes no pior caso (teto de log2 n = {Teorico})",
                    size,
                    _counter.Count,
                    (int)Math.Ceiling(Math.Log2(size + 1)));
            }
            else
            {
                _logger.LogInformation(
                    "  n = {Tamanho,-10}: {Teorico} comparacoes seriam suficientes (nao alocado aqui)",
                    size,
                    (int)Math.Ceiling(Math.Log2(size + 1)));
            }
        }

        _logger.LogInformation("Multiplicar o tamanho por mil acrescenta cerca de dez comparacoes. E essa a diferenca para a busca linear.");
    }

    private void RunOverflow()
    {
        Section("6. A armadilha do ponto medio");

        int low = int.MaxValue - 4;
        int high = int.MaxValue;

        int naive = unchecked((low + high) / 2);
        int safe = BinarySearches.Midpoint(low, high);

        _logger.LogError("  (lo + hi) / 2      -> {Resultado} (negativo: a soma estourou)", naive);
        _logger.LogInformation("  lo + (hi - lo) / 2 -> {Resultado} (correto)", safe);

        _logger.LogInformation(
            "Com vetores pequenos os dois coincidem, e por isso o bug sobrevive a testes. So aparece com indices grandes.");
    }

    private static int[] BuildRange(int size)
    {
        int[] values = new int[size];
        for (int index = 0; index < size; index++)
        {
            values[index] = index;
        }

        return values;
    }

    /// <summary>Gera todos os vetores ordenados de um tamanho, com valores de 0 a maxValue.</summary>
    private static IEnumerable<int[]> GenerateSortedArrays(int length, int maxValue)
    {
        int[] current = new int[length];

        IEnumerable<int[]> Build(int position, int minimum)
        {
            if (position == length)
            {
                yield return (int[])current.Clone();

                yield break;
            }

            for (int value = minimum; value <= maxValue; value++)
            {
                current[position] = value;

                foreach (int[] result in Build(position + 1, value))
                {
                    yield return result;
                }
            }
        }

        return Build(0, 0);
    }

    private static int LinearLowerBound(IReadOnlyList<int> sorted, int target)
    {
        for (int index = 0; index < sorted.Count; index++)
        {
            if (sorted[index] >= target)
            {
                return index;
            }
        }

        return sorted.Count;
    }

    private static int LinearUpperBound(IReadOnlyList<int> sorted, int target)
    {
        for (int index = 0; index < sorted.Count; index++)
        {
            if (sorted[index] > target)
            {
                return index;
            }
        }

        return sorted.Count;
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
