using System;
using System.Collections.Generic;

namespace YieldReturnDemo;

/// <summary>
/// Reimplementação didática de operadores do LINQ usando `yield return`.
/// Cada operador escreve um traço no console para tornar visível a ordem real
/// de execução do pipeline.
/// </summary>
public static class LazyOperators
{
    public static IEnumerable<TSource> WhereLazy<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        foreach (TSource item in source)
        {
            Console.WriteLine($"  [where]  avaliando {item}");

            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<TResult> SelectLazy<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        foreach (TSource item in source)
        {
            Console.WriteLine($"  [select] transformando {item}");
            yield return selector(item);
        }
    }

    public static IEnumerable<TSource> TakeLazy<TSource>(this IEnumerable<TSource> source, int count)
    {
        if (count <= 0)
        {
            yield break;
        }

        int taken = 0;

        foreach (TSource item in source)
        {
            yield return item;
            taken++;

            if (taken == count)
            {
                Console.WriteLine($"  [take]   limite de {count} atingido, encerrando o pipeline");
                yield break;
            }
        }
    }
}
