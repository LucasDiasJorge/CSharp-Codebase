using System.Runtime.CompilerServices;

namespace AsyncStreamsDemo;

/// <summary>
/// Operadores preguiçosos sobre <see cref="IAsyncEnumerable{T}"/>, escritos com iteradores
/// assincronos. Sao a versao async dos operadores do LINQ e mantem a mesma propriedade:
/// nada roda enquanto ninguem itera, e a fonte so avanca conforme a demanda.
///
/// Existe o pacote <c>System.Linq.Async</c> com operadores prontos; aqui eles sao escritos
/// a mao justamente para deixar a mecanica visivel.
/// </summary>
public static class AsyncOperators
{
    public static async IAsyncEnumerable<TSource> WhereAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (TSource item in source.WithCancellation(cancellationToken))
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TResult> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (TSource item in source.WithCancellation(cancellationToken))
        {
            yield return selector(item);
        }
    }

    /// <summary>
    /// Interrompe a enumeracao ao atingir a contagem. O `yield break` encerra o iterador,
    /// o que dispara o <c>DisposeAsync</c> da fonte e evita todo o trabalho restante.
    /// </summary>
    public static async IAsyncEnumerable<TSource> TakeAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            yield break;
        }

        int emitted = 0;

        await foreach (TSource item in source.WithCancellation(cancellationToken))
        {
            yield return item;
            emitted++;

            if (emitted >= count)
            {
                yield break;
            }
        }
    }
}
