using System.Numerics;
using BenchmarkTool.Lib;

namespace BenchmarkTool;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== BENCHMARK DE JOBS PESADOS ===\n");

        // Job 1: Números Primos
        Console.WriteLine("1. Calculando números primos...");
        List<long> primes = new List<long>();
        Benchmark benchmark1 = new Benchmark();
        benchmark1.Run(async () =>
        {
            primes = Job.FindPrimes(1000000, 1010000);
        });
        Console.WriteLine($"   Encontrados {primes.Count} números primos\n");

        // Job 2: Multiplicação de Matrizes
        Console.WriteLine("2. Multiplicando matrizes 500x500...");
        double[,] matrix = null;
        Benchmark benchmark2 = new Benchmark();
        benchmark2.Run(async () =>
        {
            matrix = Job.MatrixMultiplication(500);
        });
        Console.WriteLine($"   Matriz resultante: {matrix.GetLength(0)}x{matrix.GetLength(1)}\n");

        // Job 3: Hashes Criptográficos
        Console.WriteLine("3. Computando hashes SHA256...");
        List<string> hashes = new List<string>();
        Benchmark benchmark3 = new Benchmark();
        benchmark3.Run(async () =>
        {
            hashes = Job.ComputeHashes(1000);
        });
        Console.WriteLine($"   Gerados {hashes.Count} hashes\n");

        // Job 4: Fibonacci Grande
        Console.WriteLine("4. Calculando Fibonacci(100000)...");
        BigInteger fib = System.Numerics.BigInteger.Zero;
        Benchmark benchmark4 = new Benchmark();
        benchmark4.Run(async () =>
        {
            fib = Job.FibonacciBig(1000000);
        });
        Console.WriteLine($"   Resultado tem {fib.ToString().Length} dígitos\n");

        // Job 5: Ordenação Ineficiente
        Console.WriteLine("5. Ordenando lista com Bubble Sort (10000 elementos)...");
        List<int> sortedList = new List<int>();
        Benchmark benchmark5 = new Benchmark();
        benchmark5.Run(async () =>
        {
            sortedList = Job.SortLargeList(10000);
        });
        Console.WriteLine($"   Lista ordenada com {sortedList.Count} elementos\n");

        Console.WriteLine("=== BENCHMARK COMPLETO ===");
    }
}
