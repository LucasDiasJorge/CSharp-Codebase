using System;
using System.Collections.Generic;
using System.Linq;

namespace YieldReturnDemo.Demos;

/// <summary>
/// Compara a abordagem eager (materializar tudo em uma lista) com a abordagem
/// lazy (encadear iteradores). Com `yield return`, cada elemento atravessa o
/// pipeline inteiro antes de o próximo ser lido.
/// </summary>
public static class StreamingPipelineDemo
{
    private const int TotalRecords = 1_000_000;

    public static void Run()
    {
        Console.WriteLine("Pipeline lazy sobre 1..8, pegando os 2 primeiros pares dobrados:");
        List<int> result = Enumerable.Range(1, 8)
            .WhereLazy(number => number % 2 == 0)
            .SelectLazy(number => number * 2)
            .TakeLazy(2)
            .ToList();

        Console.WriteLine("Resultado: " + string.Join(", ", result));
        Console.WriteLine("Repare que o pipeline parou em 4: os números 5..8 nunca foram avaliados.");

        Console.WriteLine();
        CompareAllocations();
    }

    private static void CompareAllocations()
    {
        Console.WriteLine($"Processando {TotalRecords:N0} registros e usando apenas os 3 primeiros.");

        long eagerBefore = GC.GetTotalAllocatedBytes(precise: true);
        List<string> eagerFirstThree = LoadRecordsEager().Take(3).ToList();
        long eagerAllocated = GC.GetTotalAllocatedBytes(precise: true) - eagerBefore;

        long lazyBefore = GC.GetTotalAllocatedBytes(precise: true);
        List<string> lazyFirstThree = LoadRecordsLazy().Take(3).ToList();
        long lazyAllocated = GC.GetTotalAllocatedBytes(precise: true) - lazyBefore;

        Console.WriteLine($"    Eager (List<string> completa): {eagerAllocated:N0} bytes alocados");
        Console.WriteLine($"    Lazy  (yield return):          {lazyAllocated:N0} bytes alocados");
        Console.WriteLine($"    Mesmo resultado nos dois casos: {string.Join(", ", eagerFirstThree)} / {string.Join(", ", lazyFirstThree)}");
        Console.WriteLine("A versão eager paga por 1.000.000 de strings para usar 3; a lazy produz só o que foi pedido.");
    }

    /// <summary>
    /// Abordagem eager: constrói a coleção inteira em memória antes de retornar.
    /// </summary>
    private static List<string> LoadRecordsEager()
    {
        List<string> records = new List<string>(TotalRecords);

        for (int index = 1; index <= TotalRecords; index++)
        {
            records.Add($"registro-{index}");
        }

        return records;
    }

    /// <summary>
    /// Abordagem lazy: mesma lógica, memória constante, porque cada registro é
    /// entregue e descartado antes de o próximo ser criado.
    /// </summary>
    private static IEnumerable<string> LoadRecordsLazy()
    {
        for (int index = 1; index <= TotalRecords; index++)
        {
            yield return $"registro-{index}";
        }
    }
}
