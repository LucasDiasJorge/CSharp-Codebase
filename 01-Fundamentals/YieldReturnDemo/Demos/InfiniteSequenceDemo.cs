using System;
using System.Collections.Generic;
using System.Linq;

namespace YieldReturnDemo.Demos;

/// <summary>
/// Iteradores permitem descrever sequências sem fim e deixar o consumidor decidir
/// quanto vai consumir. Também mostra `yield break` como saída antecipada.
/// </summary>
public static class InfiniteSequenceDemo
{
    private static int producedItems;

    public static void Run()
    {
        producedItems = 0;
        List<long> firstTen = Fibonacci().Take(10).ToList();
        Console.WriteLine("Primeiros 10 termos de Fibonacci: " + string.Join(", ", firstTen));
        Console.WriteLine($"Itens realmente produzidos pelo laço infinito: {producedItems}");

        producedItems = 0;
        long firstAboveOneThousand = Fibonacci().First(value => value > 1000);
        Console.WriteLine();
        Console.WriteLine($"Primeiro termo acima de 1000: {firstAboveOneThousand}");
        Console.WriteLine($"Itens produzidos até encontrar a resposta: {producedItems}");

        Console.WriteLine();
        Console.WriteLine("yield break encerra a sequência antes do fim natural do laço:");
        IEnumerable<string> lines = ReadUntilTerminator(new[] { "linha 1", "linha 2", "FIM", "linha 4" });
        foreach (string line in lines)
        {
            Console.WriteLine($"    {line}");
        }
    }

    /// <summary>
    /// `while (true)` só é seguro porque nada é calculado antes de ser pedido.
    /// A responsabilidade de parar é de quem consome (Take, First, TakeWhile...).
    /// </summary>
    private static IEnumerable<long> Fibonacci()
    {
        long previous = 0;
        long current = 1;

        while (true)
        {
            producedItems++;
            yield return previous;

            long next = previous + current;
            previous = current;
            current = next;
        }
    }

    /// <summary>
    /// `yield break` termina a sequência imediatamente, como um `return` sem valor.
    /// </summary>
    private static IEnumerable<string> ReadUntilTerminator(IEnumerable<string> source)
    {
        foreach (string line in source)
        {
            if (line == "FIM")
            {
                Console.WriteLine("    [iterador] terminador encontrado, chamando yield break");
                yield break;
            }

            yield return line;
        }
    }
}
