using System;
using System.Collections.Generic;

namespace YieldReturnDemo.Demos;

/// <summary>
/// Ponto de partida: o uso mais simples possível de `yield return`, sem traços
/// de execução nem medições. A ideia é só responder "como se escreve" antes de
/// as próximas seções explicarem "como funciona por dentro".
/// </summary>
public static class BasicsDemo
{
    public static void Run()
    {
        Console.WriteLine("Um iterador devolve valores um a um, com yield return:");
        foreach (string color in GetColors())
        {
            Console.WriteLine($"    {color}");
        }

        Console.WriteLine();
        Console.WriteLine("Mesmo resultado da versão com List<string>, sem criar a lista:");
        foreach (string color in GetColorsWithList())
        {
            Console.WriteLine($"    {color}");
        }

        Console.WriteLine();
        Console.WriteLine("yield return também funciona dentro de um laço:");
        foreach (int number in CountTo(5))
        {
            Console.WriteLine($"    {number}");
        }

        Console.WriteLine();
        Console.WriteLine("E permite filtrar enquanto percorre outra sequência:");
        int[] numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        foreach (int number in GetEvenNumbers(numbers))
        {
            Console.WriteLine($"    {number}");
        }

        Console.WriteLine();
        Console.WriteLine("Um iterador é sempre consumido com foreach ou com métodos LINQ.");
    }

    /// <summary>
    /// A forma mais simples: cada `yield return` entrega um valor da sequência.
    /// O tipo de retorno é <see cref="IEnumerable{T}"/>, nunca <c>string</c>.
    /// </summary>
    private static IEnumerable<string> GetColors()
    {
        yield return "vermelho";
        yield return "verde";
        yield return "azul";
    }

    /// <summary>
    /// A alternativa tradicional: montar uma coleção e devolvê-la pronta.
    /// Funciona, mas exige criar a lista e preenchê-la antes de retornar.
    /// </summary>
    private static List<string> GetColorsWithList()
    {
        List<string> colors = new List<string>();
        colors.Add("vermelho");
        colors.Add("verde");
        colors.Add("azul");

        return colors;
    }

    /// <summary>
    /// Dentro de um laço, o `yield return` é executado a cada volta.
    /// </summary>
    private static IEnumerable<int> CountTo(int limit)
    {
        for (int number = 1; number <= limit; number++)
        {
            yield return number;
        }
    }

    /// <summary>
    /// Um iterador pode consumir outra sequência e produzir só o que interessa.
    /// </summary>
    private static IEnumerable<int> GetEvenNumbers(IEnumerable<int> source)
    {
        foreach (int number in source)
        {
            if (number % 2 == 0)
            {
                yield return number;
            }
        }
    }
}
