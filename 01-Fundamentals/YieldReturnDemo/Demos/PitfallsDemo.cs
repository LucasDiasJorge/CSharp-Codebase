using System;
using System.Collections.Generic;
using System.Linq;

namespace YieldReturnDemo.Demos;

/// <summary>
/// Reúne os erros mais frequentes ao usar `yield return`: reexecução silenciosa,
/// validação de argumentos adiada e limpeza de recursos em consumo parcial.
/// </summary>
public static class PitfallsDemo
{
    private static int queryExecutions;

    public static void Run()
    {
        ShowMultipleEnumeration();
        Console.WriteLine();
        ShowDeferredArgumentValidation();
        Console.WriteLine();
        ShowCleanupWithFinally();
    }

    /// <summary>
    /// Armadilha 1: cada enumeração reexecuta o iterador. Guardar o resultado em
    /// uma variável não guarda os dados, guarda a receita para produzi-los.
    /// </summary>
    private static void ShowMultipleEnumeration()
    {
        Console.WriteLine("Armadilha 1: múltiplas enumerações reexecutam o iterador.");

        queryExecutions = 0;
        IEnumerable<int> expensiveQuery = RunExpensiveQuery();
        int count = expensiveQuery.Count();
        int sum = expensiveQuery.Sum();
        int max = expensiveQuery.Max();
        Console.WriteLine($"    Count={count}, Sum={sum}, Max={max} custaram {queryExecutions} execuções da consulta.");

        queryExecutions = 0;
        List<int> materialized = RunExpensiveQuery().ToList();
        Console.WriteLine($"    Com ToList(): Count={materialized.Count}, Sum={materialized.Sum()}, Max={materialized.Max()} custaram {queryExecutions} execução.");
    }

    /// <summary>
    /// Armadilha 2: em um método iterador, nem a validação de argumentos executa
    /// na chamada. A correção é separar o método público (não iterador) do
    /// iterador privado.
    /// </summary>
    private static void ShowDeferredArgumentValidation()
    {
        Console.WriteLine("Armadilha 2: validação de argumentos adiada.");

        IEnumerable<int> broken = TakePageWrong(-1);
        Console.WriteLine("    TakePageWrong(-1) retornou sem lançar exceção.");

        try
        {
            broken.GetEnumerator().MoveNext();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            Console.WriteLine($"    A exceção só apareceu ao enumerar: {exception.GetType().Name}");
        }

        try
        {
            TakePageRight(-1);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            Console.WriteLine($"    TakePageRight(-1) lançou na chamada, como esperado: {exception.GetType().Name}");
        }
    }

    /// <summary>
    /// Armadilha 3: quando o consumidor interrompe o foreach, o bloco `finally`
    /// do iterador roda no Dispose do enumerador. É onde a liberação de recursos
    /// deve ficar.
    /// </summary>
    private static void ShowCleanupWithFinally()
    {
        Console.WriteLine("Armadilha 3: liberação de recursos em consumo parcial.");

        foreach (int value in ReadWithCleanup())
        {
            Console.WriteLine($"    consumidor recebeu {value}");

            if (value == 2)
            {
                Console.WriteLine("    consumidor interrompeu o laço");
                break;
            }
        }
    }

    private static IEnumerable<int> RunExpensiveQuery()
    {
        queryExecutions++;

        yield return 10;
        yield return 20;
        yield return 30;
    }

    /// <summary>
    /// Errado: o `throw` faz parte da máquina de estados e só dispara na enumeração.
    /// </summary>
    private static IEnumerable<int> TakePageWrong(int pageSize)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        for (int index = 1; index <= pageSize; index++)
        {
            yield return index;
        }
    }

    /// <summary>
    /// Certo: método comum valida e delega para o iterador privado.
    /// </summary>
    private static IEnumerable<int> TakePageRight(int pageSize)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        return TakePageIterator(pageSize);
    }

    private static IEnumerable<int> TakePageIterator(int pageSize)
    {
        for (int index = 1; index <= pageSize; index++)
        {
            yield return index;
        }
    }

    private static IEnumerable<int> ReadWithCleanup()
    {
        Console.WriteLine("    [iterador] recurso aberto");

        try
        {
            for (int index = 1; index <= 5; index++)
            {
                yield return index;
            }
        }
        finally
        {
            Console.WriteLine("    [iterador] finally executou: recurso liberado");
        }
    }
}
