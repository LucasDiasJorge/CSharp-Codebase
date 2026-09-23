namespace BinarySearchBoundariesDemo.Search;

/// <summary>
/// Conta comparações, para que a promessa de O(log n) seja um número e não uma
/// afirmação.
/// </summary>
public sealed class ComparisonCounter
{
    public int Count { get; private set; }

    public void Reset() => Count = 0;

    public int Compare(int left, int right)
    {
        Count++;

        return left.CompareTo(right);
    }
}

/// <summary>
/// As três buscas binárias, todas derivadas do mesmo invariante.
///
/// O intervalo é sempre <b>semiaberto</b>: <c>[lo, hi)</c>. `lo` é o primeiro índice
/// que ainda pode ser resposta; `hi` é o primeiro que já se sabe que não é. O laço roda
/// enquanto o intervalo não estiver vazio, e cada passo o encolhe — é dessa disciplina
/// que saem, sem adivinhação, o `&lt;` na condição e o `mid + 1` no ajuste.
/// </summary>
public static class BinarySearches
{
    /// <summary>
    /// Calcula o meio sem estourar. <c>(lo + hi) / 2</c> transborda quando a soma passa
    /// de <see cref="int.MaxValue"/> — bug que ficou anos no JDK e em vários livros.
    /// </summary>
    public static int Midpoint(int low, int high) => low + ((high - low) / 2);

    /// <summary>
    /// Busca exata. Devolve o índice de alguma ocorrência, ou -1.
    /// Com duplicatas, não promete qual — para isso existem as duas fronteiras abaixo.
    /// </summary>
    public static int Exact(IReadOnlyList<int> sorted, int target, ComparisonCounter counter)
    {
        int low = 0;
        int high = sorted.Count;

        while (low < high)
        {
            int mid = Midpoint(low, high);
            int comparison = counter.Compare(sorted[mid], target);

            if (comparison == 0)
            {
                return mid;
            }

            if (comparison < 0)
            {
                // sorted[mid] e pequeno demais: descarta mid e tudo a esquerda.
                low = mid + 1;
            }
            else
            {
                // sorted[mid] ja e grande demais: hi e exclusivo, entao vira mid.
                high = mid;
            }
        }

        return -1;
    }

    /// <summary>
    /// Lower bound: índice do PRIMEIRO elemento <c>&gt;= target</c>. Se não houver,
    /// devolve <c>Count</c> — que é exatamente onde o valor deveria ser inserido para
    /// manter a ordem.
    /// </summary>
    public static int LowerBound(IReadOnlyList<int> sorted, int target, ComparisonCounter counter)
    {
        int low = 0;
        int high = sorted.Count;

        while (low < high)
        {
            int mid = Midpoint(low, high);

            if (counter.Compare(sorted[mid], target) < 0)
            {
                low = mid + 1;
            }
            else
            {
                // sorted[mid] >= target: mid AINDA pode ser a resposta, entao hi = mid
                // (e nao mid - 1). Trocar por mid - 1 aqui descarta a propria resposta.
                high = mid;
            }
        }

        return low;
    }

    /// <summary>
    /// Upper bound: índice do PRIMEIRO elemento <c>&gt; target</c>. A única diferença
    /// para o lower bound é o <c>&lt;=</c> no lugar do <c>&lt;</c>.
    /// </summary>
    public static int UpperBound(IReadOnlyList<int> sorted, int target, ComparisonCounter counter)
    {
        int low = 0;
        int high = sorted.Count;

        while (low < high)
        {
            int mid = Midpoint(low, high);

            if (counter.Compare(sorted[mid], target) <= 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        return low;
    }

    /// <summary>
    /// Quantas vezes o valor aparece. Cai de graça a partir das duas fronteiras — é o
    /// principal motivo de valer a pena implementá-las em vez de só a busca exata.
    /// </summary>
    public static int CountOccurrences(IReadOnlyList<int> sorted, int target, ComparisonCounter counter)
    {
        return UpperBound(sorted, target, counter) - LowerBound(sorted, target, counter);
    }
}
