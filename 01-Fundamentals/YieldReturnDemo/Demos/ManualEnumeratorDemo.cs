using System;
using System.Collections;
using System.Collections.Generic;

namespace YieldReturnDemo.Demos;

/// <summary>
/// Compara a implementação manual de <see cref="IEnumerator{T}"/> com a versão
/// equivalente escrita com `yield return`. O resultado é o mesmo; a diferença
/// está na quantidade de código de estado que o compilador passa a gerar.
/// </summary>
public static class ManualEnumeratorDemo
{
    public static void Run()
    {
        Console.WriteLine("Contagem regressiva com enumerador escrito à mão:");
        foreach (int value in new ManualCountdown(3))
        {
            Console.WriteLine($"    {value}");
        }

        Console.WriteLine();
        Console.WriteLine("Mesma sequência com yield return:");
        foreach (int value in CountdownWithYield(3))
        {
            Console.WriteLine($"    {value}");
        }

        Console.WriteLine();
        Console.WriteLine("Iterando o iterador na mão, sem foreach, para ver a mecânica:");
        using (IEnumerator<int> enumerator = CountdownWithYield(2).GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                Console.WriteLine($"    MoveNext() == true, Current == {enumerator.Current}");
            }

            Console.WriteLine("    MoveNext() == false: a sequência acabou");
        }
    }

    /// <summary>
    /// Versão com `yield return`: o campo de estado, o controle de posição e o
    /// contrato de <see cref="IEnumerable{T}"/> ficam por conta do compilador.
    /// </summary>
    private static IEnumerable<int> CountdownWithYield(int start)
    {
        for (int value = start; value >= 1; value--)
        {
            yield return value;
        }
    }
}

/// <summary>
/// Implementação manual do mesmo comportamento, para dimensionar o que o
/// `yield return` economiza.
/// </summary>
public sealed class ManualCountdown : IEnumerable<int>
{
    private readonly int start;

    public ManualCountdown(int start)
    {
        this.start = start;
    }

    public IEnumerator<int> GetEnumerator()
    {
        return new ManualCountdownEnumerator(start);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class ManualCountdownEnumerator : IEnumerator<int>
    {
        private readonly int start;
        private int current;
        private bool started;

        public ManualCountdownEnumerator(int start)
        {
            this.start = start;
            current = 0;
            started = false;
        }

        public int Current => current;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!started)
            {
                started = true;
                current = start;
                return current >= 1;
            }

            current--;
            return current >= 1;
        }

        public void Reset()
        {
            throw new NotSupportedException("Iteradores gerados por yield return também não suportam Reset.");
        }

        public void Dispose()
        {
        }
    }
}
