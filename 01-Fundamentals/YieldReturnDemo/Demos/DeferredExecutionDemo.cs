using System;
using System.Collections.Generic;

namespace YieldReturnDemo.Demos;

/// <summary>
/// Mostra o comportamento mais importante de `yield return`: o corpo do método
/// não executa na chamada, e sim a cada `MoveNext()` do consumidor.
/// </summary>
public static class DeferredExecutionDemo
{
    public static void Run()
    {
        Console.WriteLine("Chamando GenerateNumbers(3)...");
        IEnumerable<int> numbers = GenerateNumbers(3);
        Console.WriteLine("A chamada retornou e NENHUMA linha do corpo executou ainda.");
        Console.WriteLine();

        Console.WriteLine("Primeiro foreach (o corpo executa em pedaços, pausando em cada yield):");
        foreach (int number in numbers)
        {
            Console.WriteLine($"    [consumidor] recebeu {number}");
        }

        Console.WriteLine();
        Console.WriteLine("Segundo foreach sobre a MESMA variável: o corpo roda de novo, do zero.");
        foreach (int number in numbers)
        {
            Console.WriteLine($"    [consumidor] recebeu {number}");
        }

        Console.WriteLine();
        Console.WriteLine("Consumo parcial: o corpo para no primeiro yield e nunca chega ao fim.");
        foreach (int number in numbers)
        {
            Console.WriteLine($"    [consumidor] recebeu {number} e vai interromper o laço");
            break;
        }
    }

    /// <summary>
    /// Um método com `yield return` é um iterador: o compilador transforma o corpo
    /// em uma máquina de estados que lembra onde parou entre uma chamada e outra.
    /// </summary>
    private static IEnumerable<int> GenerateNumbers(int count)
    {
        Console.WriteLine("  [iterador] corpo iniciou");

        for (int index = 1; index <= count; index++)
        {
            Console.WriteLine($"  [iterador] vai produzir {index}");
            yield return index;
            Console.WriteLine($"  [iterador] retomou a execução depois de entregar {index}");
        }

        Console.WriteLine("  [iterador] corpo terminou");
    }
}
